using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.PowerCore;
using SparkLevel103Process = ProjectSpark.Gameplay.PowerCore.SparkLevel103Process;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.PowerCore
{
    internal static partial class SparkLevel103Builder
    {
        private static readonly string[] DriverTasks = { "W017", "W018", "W041", "W042" };

        private static void ConfigureBoard(SparkLevel103Process board, Circuit graph, string[][] plans,
            IEnumerable<string> barrelIds, Dictionary<string, Material> mats, List<string> csv)
        {
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 12f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity source reference", board.input, board.ground, 10000f);
            board.aux33 = graph.Pin("U6.AUX_3V3");
            board.aux18 = graph.Pin("U7.AUX_1V8");
            board.drivers = new SparkTerminal[4];
            var remaining = new Queue<string>(barrelIds.Where(id => id != "V017"));
            var groups = plans.GroupBy(row => row[1]).ToArray();
            var routes = new List<SparkLevel103Process.Route>();
            var witnesses = new List<SparkTerminal>();
            var pads = new List<Vector3>();
            var placements = board.parts.Select(p => graph.Root.InverseTransformPoint(p.footprint.position)).ToArray();
            Vector3 Site(string id) => placements[Array.FindIndex(board.parts, part => part.label == id)];
            for (int group = 0; group < groups.Length; group++)
            {
                int viasLeft = group == 0 ? 4 : 5;
                SparkTerminal previous = board.input;
                foreach (var task in groups[group])
                {
                    int index = routes.Count;
                    string id = task[0];
                    int phase = Array.IndexOf(DriverTasks, id);
                    bool driver = phase >= 0;
                    var site = placements[index % placements.Length];
                    var entry = FindPad(driver ? Site("U" + (phase + 2)) : site, index >= placements.Length ? 3 : 0, pads);
                    var destination = FindPad(driver ? Site("L" + (phase + 1)) : site, 2, pads);
                    string net = driver ? "PH" + (phase + 1) + "_VCORE_PATH" : "CHECK_" + (group + 1);
                    var a = graph.Pin(driver ? net + ".DRIVE_TEST" : id + ".A", entry, true);
                    var b = graph.Pin(driver ? net + ".INDUCTOR_OUT" : id + ".B", destination, true);
                    var route = new SparkLevel103Process.Route
                    {
                        id = id, group = groups[group].Key, net = net, entry = entry, destination = destination,
                        layer = driver ? 4 : (index + group) % 8 + 1, from = a, to = b,
                        copper = Copper(graph.Root, mats[driver ? "Gold_ENIG" : "Copper"], id, entry, destination)
                    };
                    var assemblyOut = driver ? graph.Pin(id + ".ASSEMBLY_CHECK") : a;
                    var joint = graph.SwitchedOutput(id + " assembly solder contact", previous, assemblyOut, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = joint;
                    previous = driver ? assemblyOut : b;
                    if (driver)
                    {
                        graph.Load(net + " drive-test feed", board.aux33, a, 68f);
                        var receiver = b;
                        if (phase == 2)
                        {
                            board.phase3Route = index;
                            receiver = graph.Pin("PH3.VCORE_L5", destination);
                            route.viaId = "V017";
                            route.exitLayer = 5;
                            route.barrel = graph.SwitchedOutput("V017 Phase 3 output span", b, receiver);
                        }
                        graph.Load(net + " test return", receiver, board.ground, 10000f);
                        board.drivers[phase] = receiver;
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
            if (remaining.Count != 0 || routes.Count != SparkLevel103Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel103Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != 8 || witnesses.Count != 6 ||
                board.parts.Any(part => !part.contact) || routes[board.phase3Route].viaId != "V017" || board.drivers.Any(p => !p))
                throw new InvalidOperationException("CPX-11 eight-layer route and phase assignment is incomplete.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
        }

        private static void ConfigureController(SparkLevel103Process board, Circuit graph,
            Dictionary<string, Material> mats, List<string> csv)
        {
            board.standby = graph.Pin("U8.PROTECTED_12V");
            board.bench = graph.SwitchedOutput("J1 protected bench disconnect", board.input, board.standby);
            graph.Load("Protected input monitor", board.standby, board.ground, 330f);
            var logicSource = graph.Pin("U6 logic source");
            graph.Supply("U6 training logic regulator", logicSource, board.ground, 3.3f);
            board.logicGate = graph.SwitchedOutput("U6 logic rail gate", logicSource, board.aux33);
            graph.Load("Logic return", board.aux33, board.ground, 150f);

            var auxSource = graph.Pin("U7 auxiliary source");
            graph.Supply("U7 training auxiliary regulator", auxSource, board.ground, 1.8f);
            board.auxGate = graph.SwitchedOutput("U7 auxiliary rail gate", auxSource, board.aux18);
            graph.Load("Auxiliary return", board.aux18, board.ground, 150f);

            Vector3 Pad(string id, Vector3 offset) =>
                graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == id).footprint.position) + offset;
            Vector3 from = Pad("U10", new Vector3(.009f, .0030f, .005f));
            Vector3 to = Pad("U11", new Vector3(-.008f, .0030f, -.004f));
            var sender = graph.Pin("U10.TRUSTED_VCORE_GOOD_OUT", from, true);
            var receiver = graph.Pin("U11.TRUSTED_VCORE_GOOD_IN", to, true);
            board.heartbeat = graph.SwitchedOutput("U10 validity pulse driver", board.aux33, sender);
            var bias = graph.Load("U11 receiver bias", receiver, board.ground, 10000f);
            receiver.transform.SetParent(bias.transform, true);
            board.jumper = new SparkLevel103Process.Route
            {
                id = "W117", net = "TRUSTED_VCORE_GOOD", group = "Independent safety", layer = 1,
                from = sender, to = receiver, entry = from, destination = to,
                copper = Copper(graph.Root, mats["Gold_ENIG"], "W117", from, to)
            };
            csv.Add(Row(board.jumper));
            foreach (var point in new[] { from, to })
                Primitive(graph.Root, "W117 contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0025f, .00022f, .0025f), mats["Gold_ENIG"]);

            board.monitorSupply = graph.Pin("U11.INDEPENDENT_3V3");
            var supervisorFeed = graph.Load("Independent supervisor supply", board.aux33, board.monitorSupply, 1f);
            board.monitorSupply.transform.SetParent(supervisorFeed.transform, true);
            graph.Load("Independent supervisor bias", board.monitorSupply, board.ground, 1000f);
            board.protectedFeed = graph.Pin("U11.PROTECTED_TX");
            board.workload = graph.SwitchedOutput("U11 independent transmission permit", board.monitorSupply, board.protectedFeed);
            graph.Load("Protected TX permit witness", board.protectedFeed, board.ground, 330f);

            board.startMarker = Marker(graph.Root, "Source locator", mats["Gold_ENIG"], .0026f);
            board.endMarker = Marker(graph.Root, "Destination locator", mats["LED_Green"], .0026f);
            board.locationMarker = Marker(graph.Root, "Placement locator", mats["LED_Amber"], .0049f);
            board.statusLamp = Primitive(graph.Root, "U11 permit lamp", PrimitiveType.Cube,
                to + new Vector3(-.003f, .001f, 0), new Vector3(.0018f, .0006f, .0012f), mats["LED_Green"]);
            var risks = new[] { Pad("R2", Vector3.zero), board.routes[board.phase3Route].destination, Pad("Q7", Vector3.zero) };
            board.faultOverlays = risks.Select((point, index) =>
            {
                var marker = Marker(graph.Root, "Fault locator " + (index + 1), mats["Warning_Red"], .005f);
                marker.transform.localPosition = point + Vector3.up * .004f;
                marker.enabled = true;
                marker.gameObject.SetActive(false);
                return marker.gameObject;
            }).ToArray();
            BoardText(graph.Root, "CPX-11  /  THE POWER CORE", new Vector3(-.081f, .0045f, -.076f), .00075f, new Color(.87f, .92f, 1f));
            BoardText(graph.Root, "U11 / W117", to + new Vector3(-.006f, .0021f, .006f), .00040f, Color.white);
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
                    if (Mathf.Abs(candidate.x) > .116f || Mathf.Abs(candidate.z) > .086f ||
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
            line.widthMultiplier = name == "W117" ? .041f : .023f;
            line.positionCount = 3;
            line.SetPosition(0, a);
            line.SetPosition(1, new Vector3(b.x, Mathf.Max(a.y, b.y) + (name == "W117" ? .005f : 0f), a.z));
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
