using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.AutoLevelGenerator;
using ProjectSpark.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    internal static partial class SparkLevel99Builder
    {
        private static void ConfigureBoard(SparkLevel99Process board, SparkAutoLevelBlueprint blueprint, Circuit graph,
            Dictionary<string, Material> materials, List<string> csv)
        {
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 5f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity source reference", board.input, board.ground, 10000f);
            board.vdd = graph.Pin("MCU_3V3.TP");
            board.spiSense = graph.Pin("U2.SPI_DATA_WITNESS", new Vector3(.038f, .0030f, -.020f));
            var bias = graph.Load("U2 SPI receiver bias", board.spiSense, board.ground, 10000f);
            board.spiSense.transform.SetParent(bias.transform, true);
            var designVias = blueprint.Vias.Where(item => item.Id != "V11").ToArray();
            var routes = new List<SparkLevel99Process.Route>();
            var witnesses = new List<SparkTerminal>();
            var used = new List<Vector3>();
            var groups = blueprint.Routes.GroupBy(route => route.Subsystem).ToArray();
            var kit = board.parts.Select(part => graph.Root.InverseTransformPoint(part.footprint.position)).ToArray();
            int nextVia = 0;
            for (int group = 0; group < groups.Length; group++)
            {
                int inGroup = 0;
                SparkTerminal previous = board.input;
                foreach (var task in groups[group])
                {
                    int index = routes.Count;
                    bool spi = task.Id == "W045";
                    var site = spi ? new Vector3(.028f, .0035f, -.023f) : kit[index % kit.Length];
                    var pointA = FindPad(site, index >= kit.Length ? 3 : 0, used);
                    var pointB = FindPad(site, 2, used);
                    var a = graph.Pin(spi ? "U1.SPI_DATA" : task.Id + ".A", pointA, true);
                    var b = graph.Pin(spi ? "V11.SPI_DATA_ENTRY" : task.Id + ".B", pointB, true);
                    var route = new SparkLevel99Process.Route
                    {
                        id = task.Id, group = groups[group].Key,
                        layer = (index + group) % 4 + 1, from = a, to = b,
                        copper = Copper(graph.Root, materials["Copper"], task.Id, pointA, pointB)
                    };
                    if (spi)
                    {
                        board.spiRoute = index;
                        route.viaId = "V11";
                        route.layer = 1;
                        route.exitLayer = 4;
                        route.barrel = graph.Contact("V11 actual SPI data via", b, board.spiSense);
                        b.transform.SetParent(route.barrel.transform, true);
                        graph.Load("U1 SPI logic driver", board.vdd, a, 68f);
                        a.transform.SetParent(route.barrel.transform, true);
                        board.parts[index].contact = graph.SwitchedOutput("C18 training assembly contact", previous, graph.Pin("W045.ASSEMBLY_CHECK"));
                        previous = board.parts[index].contact.OutputTerminal;
                    }
                    else
                    {
                        var joint = graph.SwitchedOutput(task.Id + " assembly contact", previous, a, index >= board.parts.Length);
                        if (index < board.parts.Length) board.parts[index].contact = joint;
                        previous = b;
                        if (inGroup < 3)
                        {
                            route.viaId = designVias[nextVia++].Id;
                            route.layer = (index + group) % 3 + 1;
                            route.exitLayer = 4;
                            previous = graph.Pin(route.viaId + ".L4", pointB);
                            route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", b, previous);
                            b.transform.SetParent(route.barrel.transform, true);
                        }
                    }
                    routes.Add(route);
                    csv.Add(Row(route, pointA, pointB));
                    inGroup++;
                }
                graph.Load(groups[group].Key + " training continuity witness", previous, board.ground, 1000f);
                witnesses.Add(previous);
            }
            if (nextVia != 17 || routes.Count != 82 || witnesses.Count != 6 ||
                routes.Select(route => route.layer).Distinct().Count() != 4 || board.parts.Any(part => !part.contact) ||
                !routes.Any(route => route.viaId == "V11" && route.id == "W045"))
                throw new InvalidOperationException("SBC-7 schematic contract requires 62 contacts, 82 routes, 18 vias and four layers.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
        }

        private static void ConfigureController(SparkLevel99Process board, Circuit graph,
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
            var good = graph.Pin("C1.GOOD_STORAGE");
            var bad = graph.Pin("C1.WRONG_VALUE_STORAGE");
            board.healthyClockCap = graph.SwitchedOutput("C1 correct capacitor contact", board.clockSense, good, true);
            board.faultyClockCap = graph.SwitchedOutput("C1 incorrect capacitor contact", board.clockSense, bad);
            graph.Cap("C1 effective good startup capacitor", good, board.ground, .067f);
            graph.Cap("C1 effective incorrect startup capacitor", bad, board.ground, 1.40f);
            graph.Load("Inactive good capacitor bleed", good, board.ground, 500f);
            graph.Load("Inactive fault capacitor bleed", bad, board.ground, 500f);

            var resetIn = graph.Pin("RESET_PULLUP_INPUT");
            board.resetRelease = graph.SwitchedOutput("Independent reset supervisor", board.vdd, resetIn);
            board.resetSense = graph.Pin("U1.RESET_N");
            board.resetPullup = graph.Load("R18 native reset pull-up", resetIn, board.resetSense, 180f);
            board.resetSense.transform.SetParent(board.resetPullup.transform, true);
            graph.Load("Reset input pulldown", board.resetSense, board.ground, 1120f);

            SparkSwitch Loop(string name, out SparkTerminal pin)
            {
                pin = graph.Pin(name + ".WITNESS");
                var driver = graph.SwitchedOutput(name + " driver", board.vdd, pin);
                graph.Load(name + " input bias", pin, board.ground, 10000f);
                return driver;
            }
            board.gpioDrive = Loop("GPIO_LOOPBACK", out board.gpioSense);
            board.i2cClockDrive = Loop("I2C_SCL", out board.i2cClock);
            board.i2cDataDrive = Loop("I2C_SDA", out board.i2cData);

            Transform slot(string id) => board.parts.Single(part => part.label == id).footprint;
            Vector3 a = graph.Root.InverseTransformPoint(slot("U1").position) + new Vector3(-.010f, .0040f, .007f);
            Vector3 b = graph.Root.InverseTransformPoint(slot("U8").position) + new Vector3(.007f, .0040f, -.006f);
            var sender = graph.Pin("U1.APPLICATION_HEARTBEAT_OUT", a, true);
            var receiver = graph.Pin("U8.TRUSTED_HEARTBEAT_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U1 application heartbeat driver", board.vdd, sender);
            var sink = graph.Load("U8 trusted receiver bias", receiver, board.ground, 10000f);
            receiver.transform.SetParent(sink.transform, true);
            board.jumper = new SparkLevel99Process.Route
            {
                id = "W083", group = "INDEPENDENT HEARTBEAT SAFETY", layer = 1, from = sender, to = receiver,
                copper = Copper(graph.Root, materials["ENIG_Gold"], "W083", a, b)
            };
            foreach (var endpoint in new[] { a, b })
                Primitive(graph.Root, "W083 contact", PrimitiveType.Cylinder, endpoint - Vector3.up * .0004f,
                    new Vector3(.0026f, .00024f, .0026f), materials["ENIG_Gold"]);
            csv.Add(Row(board.jumper, a, b));
            board.protectedFeed = graph.Pin("U8.PROTECTED_WORKLOAD");
            board.workload = graph.SwitchedOutput("U8 independent output permit", board.vdd, board.protectedFeed);
            graph.Load("Protected workload indicator", board.protectedFeed, board.ground, 330f);

            board.locationMarker = Marker(graph.Root, "Placement locator", materials["LED_Green"], .0055f);
            board.startMarker = Marker(graph.Root, "Source locator", materials["ENIG_Gold"], .0029f);
            board.endMarker = Marker(graph.Root, "Destination locator", materials["LED_Green"], .0029f);
            board.statusLamp = Primitive(graph.Root, "U8 protected output lamp", PrimitiveType.Cube,
                b + new Vector3(.003f, .001f, 0), new Vector3(.0018f, .0006f, .0012f), materials["LED_Green"]);
            BoardText(graph.Root, "SBC-7  /  SILICON BRAIN", new Vector3(-.062f, .00245f, -.056f), .00063f, new Color(.83f, .92f, .90f));
            BoardText(graph.Root, "U1", graph.Root.InverseTransformPoint(slot("U1").position) + new Vector3(.006f, .0031f, .016f), .00022f, Color.white);
            BoardText(graph.Root, "U8 / W083", b + new Vector3(-.003f, .0005f, .005f), .00026f, Color.white);
            WorkLight(graph.Root, "Board key", new Vector3(-.046f, .057f, -.035f), new Color(1f, .88f, .74f), 5.3f);
            WorkLight(graph.Root, "Logic bench fill", new Vector3(.057f, .049f, .027f), new Color(.66f, .86f, 1f), 4.55f);
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
                    if (Mathf.Abs(candidate.x) > .092f || Mathf.Abs(candidate.z) > .063f ||
                        used.Exists(point => Vector2.SqrMagnitude(new Vector2(point.x - candidate.x, point.z - candidate.z)) < .0000022f)) continue;
                    used.Add(candidate);
                    return candidate;
                }
            }
            throw new InvalidOperationException("No room for SBC-7 training contacts at " + site);
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
            line.widthMultiplier = name == "W083" ? .041f : .023f;
            line.positionCount = 3;
            line.SetPosition(0, a);
            line.SetPosition(1, new Vector3(b.x, Mathf.Max(a.y, b.y) + (name == "W083" ? .005f : 0f), a.z));
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
