using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.AutoLevelGenerator;
using ProjectSpark.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    internal static partial class SparkLevel100Builder
    {
        private static readonly (string task, string net, string target)[] BusRoutes =
        {
            ("W015", "I2C_SDA", "U2_U3_U4"), ("W016", "I2C_SCL", "U2_U3_U4"),
            ("W035", "SPI_SCK", "U6_U7"), ("W036", "SPI_MOSI", "U6_U7"),
            ("W037", "SPI_MISO", "U1"), ("W038", "SPI_CS0_N", "U6"),
            ("W039", "SPI_CS1_N", "U7"), ("W055", "UART_TX", "U8"),
            ("W056", "UART_RX", "U1")
        };

        private static void ConfigureBoard(SparkLevel100Process board, SparkAutoLevelBlueprint blueprint, Circuit graph,
            Dictionary<string, Material> materials, List<string> csv)
        {
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 5f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity source reference", board.input, board.ground, 10000f);
            board.vdd = graph.Pin("MCU_3V3.TP");
            board.ports = new SparkLevel100Process.BusPort[BusRoutes.Length];

            var designVias = new Queue<string>(blueprint.Vias.Where(item => item.Id != "V11").Select(item => item.Id));
            var routes = new List<SparkLevel100Process.Route>();
            var witnesses = new List<SparkTerminal>();
            var used = new List<Vector3>();
            var groups = blueprint.Routes.GroupBy(route => route.Subsystem).ToArray();
            var kit = board.parts.Select(part => graph.Root.InverseTransformPoint(part.footprint.position)).ToArray();
            Vector3 Slot(string id) => kit[Array.FindIndex(board.parts, part => part.label == id)];
            for (int group = 0; group < groups.Length; group++)
            {
                int remainingVias = group == 0 ? 4 : 3;
                SparkTerminal previous = board.input;
                foreach (var task in groups[group])
                {
                    int index = routes.Count;
                    int portIndex = Array.FindIndex(BusRoutes, item => item.task == task.Id);
                    bool bus = portIndex >= 0;
                    bool sck = portIndex == 2;
                    bool returning = portIndex is 4 or 8;
                    var site = kit[index % kit.Length];
                    var name = bus ? BusRoutes[portIndex].net : task.Id;
                    var sourceSite = bus ? Slot(returning ? portIndex == 4 ? "U6" : "U8" : "U1") : site;
                    var targetSite = bus ? Slot(BusRoutes[portIndex].target.Split('_')[0]) : site;
                    var pointA = FindPad(sourceSite, portIndex >= 0 ? portIndex : index >= kit.Length ? 3 : 0, used);
                    var pointB = FindPad(targetSite, bus ? portIndex + 3 : 2, used);
                    var a = graph.Pin(name + (bus ? ".DRIVE" : ".A"), pointA, true);
                    var b = graph.Pin(name + (sck ? ".V11_ENTRY" : bus ? ".RECEIVE" : ".B"), pointB, true);
                    var route = new SparkLevel100Process.Route
                    {
                        id = task.Id, group = groups[group].Key, net = bus ? name : "CHECK_" + (group + 1),
                        layer = bus ? 1 : (index + group) % 4 + 1, from = a, to = b,
                        copper = Copper(graph.Root, materials["Copper"], task.Id, pointA, pointB)
                    };

                    var assemblyOutput = bus ? graph.Pin(task.Id + ".ASSEMBLY_CHECK") : a;
                    var joint = graph.SwitchedOutput(task.Id + " solder contact", previous, assemblyOutput, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = joint;
                    previous = bus ? assemblyOutput : b;
                    if (bus)
                    {
                        var receiver = b;
                        if (sck)
                        {
                            receiver = graph.Pin("U6_U7.SPI_SCK", pointB);
                            board.spiRoute = index;
                            route.viaId = "V11";
                            route.exitLayer = 4;
                            route.barrel = graph.SwitchedOutput("V11 SPI clock barrel", b, receiver);
                        }
                        board.ports[portIndex] = CreateBusPort(board, graph, portIndex, name, a, receiver);
                    }
                    else if (remainingVias > 0)
                    {
                        remainingVias--;
                        route.viaId = designVias.Dequeue();
                        route.layer = (index + group) % 3 + 1;
                        route.exitLayer = 4;
                        previous = graph.Pin(route.viaId + ".L4", pointB);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", b, previous);
                    }
                    routes.Add(route);
                    csv.Add(Row(route, pointA, pointB));
                }
                graph.Load(groups[group].Key + " continuity witness", previous, board.ground, 1000f);
                witnesses.Add(previous);
            }
            if (designVias.Count != 0 || routes.Count != SparkLevel100Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel100Process.ViaCount ||
                witnesses.Count != 6 || routes.Select(route => route.layer).Distinct().Count() != 4 ||
                board.parts.Any(part => !part.contact) || board.ports.Any(port => port == null) ||
                !routes.Any(route => route.viaId == "V11" && route.id == "W035"))
                throw new InvalidOperationException("DNX-8 training route, via or port assignment is incomplete.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
        }

        private static SparkLevel100Process.BusPort CreateBusPort(SparkLevel100Process board, Circuit graph, int index,
            string name, SparkTerminal source, SparkTerminal receiver)
        {
            var port = new SparkLevel100Process.BusPort { sender = source, receiver = receiver };
            graph.Load(name + " receive bias", receiver, board.ground, index < 2 ? 100000f : 10000f);
            if (index < 2)
            {
                var pullup = graph.Load(index == 0 ? "R1 SDA pull-up" : "R2 SCL pull-up", board.vdd, source, 2200f);
                if (index == 0) board.i2cPullup = pullup;
                port.sink = graph.Contact(name + " open-drain driver", source, board.ground);
                var storage = graph.Pin(name + ".CAP");
                graph.Contact(name + " capacitive load contact", receiver, storage, true);
                graph.Cap(name + " accelerated line capacitance", storage, board.ground, .000045f);
            }
            else
            {
                graph.Load(name + " source bias", source, board.ground, 10000f);
                port.drive = graph.Contact(name + " logic driver", board.vdd, source);
            }
            return port;
        }

        private static void ConfigureController(SparkLevel100Process board, Circuit graph,
            Dictionary<string, Material> materials, List<string> csv)
        {
            board.standby = graph.Pin("PROTECTED_5V_IN");
            board.bench = graph.SwitchedOutput("J1 guarded bench disconnect", board.input, board.standby);
            graph.Load("5 V input witness", board.standby, board.ground, 220f);
            var regulatorSource = graph.Pin("U4_3V3_REGULATOR");
            graph.Supply("U4 training regulator", regulatorSource, board.ground, 3.3f);
            var ramp = graph.Pin("U4_SERIES_OUTPUT");
            board.regulator = graph.SwitchedOutput("U4 protected regulator gate", regulatorSource, ramp);
            var series = graph.Load("3V3 source impedance", ramp, board.vdd, .045f);
            board.vdd.transform.SetParent(series.transform, true);
            graph.Cap("MCU rail effective decoupling", board.vdd, board.ground, .11f);
            graph.Load("MCU rail bleed", board.vdd, board.ground, 120f);
            board.swdSense = graph.Pin("J4.SWD_LINK");
            var swd = graph.Load("SWD witness impedance", board.vdd, board.swdSense, 68f);
            board.swdSense.transform.SetParent(swd.transform, true);
            graph.Load("SWD debug sense bias", board.swdSense, board.ground, 10000f);

            var clockInput = graph.Pin("Y1_STARTUP_FEED");
            board.clockDrive = graph.SwitchedOutput("Y1 clock bias enable", board.vdd, clockInput);
            board.clockSense = graph.Pin("Y1.REFERENCE_ENVELOPE");
            var clockBias = graph.Load("Y1 bias and source impedance", clockInput, board.clockSense, 1f);
            board.clockSense.transform.SetParent(clockBias.transform, true);
            graph.Load("Y1 load bleed", board.clockSense, board.ground, 100f);
            var clockDrain = graph.Pin("Y1.RESET_DISCHARGE");
            board.clockDischarge = graph.SwitchedOutput("Y1 discharge contact", board.clockSense, clockDrain, true);
            graph.Load("Y1 discharge impedance", clockDrain, board.ground, .47f);
            var clockStorage = graph.Pin("Y1.STARTUP_STORAGE");
            graph.Contact("Y1 load contact", board.clockSense, clockStorage, true);
            graph.Cap("Y1 effective startup capacitance", clockStorage, board.ground, .067f);

            var resetIn = graph.Pin("RESET_PULLUP_INPUT");
            board.resetRelease = graph.SwitchedOutput("Independent reset supervisor", board.vdd, resetIn);
            board.resetSense = graph.Pin("U1.RESET_N");
            var resetPullup = graph.Load("R18 native reset pull-up", resetIn, board.resetSense, 180f);
            board.resetSense.transform.SetParent(resetPullup.transform, true);
            graph.Load("Reset input pulldown", board.resetSense, board.ground, 1120f);

            Transform slot(string id) => board.parts.Single(part => part.label == id).footprint;
            Vector3 a = graph.Root.InverseTransformPoint(slot("U1").position) + new Vector3(-.010f, .0040f, .007f);
            Vector3 b = graph.Root.InverseTransformPoint(slot("U9").position) + new Vector3(.007f, .0040f, -.006f);
            var sender = graph.Pin("U1.TRUSTED_COMM_HEALTH_OUT", a, true);
            var receiver = graph.Pin("U9.TRUSTED_COMM_HEALTH_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U1 application heartbeat driver", board.vdd, sender);
            var sink = graph.Load("U9 trusted receiver bias", receiver, board.ground, 10000f);
            receiver.transform.SetParent(sink.transform, true);
            board.jumper = new SparkLevel100Process.Route
            {
                id = "W093", group = "Independent safety", net = "COMM_HEALTH", layer = 1, from = sender, to = receiver,
                copper = Copper(graph.Root, materials["ENIG_Gold"], "W093", a, b)
            };
            foreach (var endpoint in new[] { a, b })
                Primitive(graph.Root, "W093 contact", PrimitiveType.Cylinder, endpoint - Vector3.up * .0004f,
                    new Vector3(.0026f, .00024f, .0026f), materials["ENIG_Gold"]);
            csv.Add(Row(board.jumper, a, b));
            board.protectedFeed = graph.Pin("U9.PROTECTED_WORKLOAD");
            board.workload = graph.SwitchedOutput("U9 independent output permit", board.vdd, board.protectedFeed);
            graph.Load("Protected workload indicator", board.protectedFeed, board.ground, 330f);

            board.locationMarker = Marker(graph.Root, "Placement locator", materials["LED_Green"], .0055f);
            board.startMarker = Marker(graph.Root, "Source locator", materials["ENIG_Gold"], .0029f);
            board.endMarker = Marker(graph.Root, "Destination locator", materials["LED_Green"], .0029f);
            board.statusLamp = Primitive(graph.Root, "U9 protected output lamp", PrimitiveType.Cube,
                b + new Vector3(.003f, .001f, 0), new Vector3(.0018f, .0006f, .0012f), materials["LED_Green"]);
            BoardText(graph.Root, "DNX-8  /  DIGITAL NEXUS", new Vector3(-.081f, .00245f, -.060f), .00063f, new Color(.83f, .92f, .90f));
            BoardText(graph.Root, "U1", graph.Root.InverseTransformPoint(slot("U1").position) + new Vector3(.006f, .0031f, .016f), .00022f, Color.white);
            BoardText(graph.Root, "U9 / W093", b + new Vector3(-.003f, .0005f, .005f), .00026f, Color.white);
            WorkLight(graph.Root, "Board key", new Vector3(-.046f, .057f, -.035f), new Color(1f, .89f, .77f), 6.4f);
            WorkLight(graph.Root, "Logic bench fill", new Vector3(.057f, .049f, .027f), new Color(.76f, .88f, 1f), 5.9f);
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
                    if (Mathf.Abs(candidate.x) > .101f || Mathf.Abs(candidate.z) > .068f ||
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
            line.widthMultiplier = name == "W093" ? .041f : .023f;
            line.positionCount = 3;
            line.SetPosition(0, a);
            line.SetPosition(1, new Vector3(b.x, Mathf.Max(a.y, b.y) + (name == "W093" ? .005f : 0f), a.z));
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
