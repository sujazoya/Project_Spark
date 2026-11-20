using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.AutoLevelGenerator;
using ProjectSpark.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    internal static partial class SparkLevel98Builder
    {
        private static void ConfigureBoard(SparkLevel98Process board, SparkAutoLevelBlueprint blueprint, Circuit graph,
            Dictionary<string, Material> materials, List<string> csv)
        {
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 12f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity source reference", board.input, board.ground, 10000f);
            board.actualRail = graph.Pin("LOAD_PLUS.TP", new Vector3(.071f, .0030f, .012f), true);
            board.loadReturn = graph.Pin("LOAD_RETURN.TP", new Vector3(.072f, .0030f, -.013f), true);
            board.sense = graph.Pin("U8.KELVIN_PLUS", new Vector3(-.010f, .0033f, -.027f), true);
            var probe = graph.Load("Kelvin differential probe bias", board.sense, board.loadReturn, 10000f);
            board.sense.transform.SetParent(probe.transform, true);
            var backup = graph.Load("Backup return resistance", board.loadReturn, board.ground, board.profile.returnBypassOhms);
            board.loadReturn.transform.SetParent(backup.transform, true);

            var groups = blueprint.Routes.GroupBy(route => route.Subsystem).ToArray();
            var routes = new List<SparkLevel98Process.Route>();
            var witnesses = new List<SparkTerminal>();
            var used = new[] { board.actualRail, board.loadReturn, board.sense }
                .Select(pin => graph.Root.InverseTransformPoint(pin.transform.position)).ToList();
            var kit = board.parts.Select(part => graph.Root.InverseTransformPoint(part.footprint.position)).ToArray();
            const string groundId = "W056", senseId = "W074";
            int nextVia = 0;
            for (int group = 0; group < groups.Length; group++)
            {
                int inGroup = 0;
                SparkTerminal previous = board.input;
                foreach (var task in groups[group])
                {
                    int index = routes.Count;
                    bool groundRoute = task.Id == groundId, senseRoute = task.Id == senseId;
                    var site = kit[index % kit.Length];
                    SparkTerminal a = groundRoute ? board.loadReturn : senseRoute ? board.actualRail :
                        graph.Pin(task.Id + ".A", FindPad(site, index >= kit.Length ? 3 : 0, used), true);
                    SparkTerminal b = senseRoute ? board.sense :
                        graph.Pin(task.Id + ".B", FindPad(groundRoute ? graph.Root.InverseTransformPoint(a.transform.position) : site, 2, used), true);
                    Vector3 start = graph.Root.InverseTransformPoint(a.transform.position);
                    Vector3 end = graph.Root.InverseTransformPoint(b.transform.position);
                    var route = new SparkLevel98Process.Route
                    {
                        id = task.Id, group = groups[group].Key, layer = groundRoute ? 1 : senseRoute ? 4 : (index + group) % 4 + 1,
                        from = a, to = b, copper = Copper(graph.Root, materials["Copper"], task.Id, start, end)
                    };
                    if (groundRoute)
                    {
                        board.groundRoute = index;
                        route.viaId = blueprint.Vias[^1].Id;
                        route.exitLayer = 4;
                        route.barrel = graph.Contact(route.viaId + " dedicated plated return", b, board.ground);
                        b.transform.SetParent(route.barrel.transform, true);
                        board.parts[index].contact = graph.SwitchedOutput("W056 assembly check", previous, graph.Pin("W056.ASSEMBLY_CHECK"));
                        previous = board.parts[index].contact.OutputTerminal;
                    }
                    else if (senseRoute) board.senseRoute = index;
                    else
                    {
                        var contact = graph.SwitchedOutput(task.Id + " board assembly contact", previous, a, index >= board.parts.Length);
                        if (index < board.parts.Length) board.parts[index].contact = contact;
                        previous = b;
                        if (inGroup < 3)
                        {
                            route.viaId = blueprint.Vias[nextVia++].Id;
                            route.layer = (index + group) % 3 + 1;
                            route.exitLayer = 4;
                            previous = graph.Pin(route.viaId + ".L4", end);
                            route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", b, previous);
                            b.transform.SetParent(route.barrel.transform, true);
                        }
                    }
                    routes.Add(route);
                    csv.Add(Row(route, start, end));
                    inGroup++;
                }
                graph.Load(groups[group].Key + " continuity witness", previous, board.ground, 1000f);
                witnesses.Add(previous);
            }
            if (nextVia != 15 || routes.Count != 74 || routes.Select(route => route.layer).Distinct().Count() != 4 ||
                board.parts.Any(part => !part.contact) || routes[board.groundRoute].id != groundId || routes[board.senseRoute].id != senseId)
                throw new InvalidOperationException("PIS-6 training graph requires 56 assembly contacts, 74 routes, 16 vias, four layers and two solved fault paths.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
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
                    if (Mathf.Abs(candidate.x) > .086f || Mathf.Abs(candidate.z) > .056f ||
                        used.Exists(point => Vector2.SqrMagnitude(new Vector2(point.x - candidate.x, point.z - candidate.z)) < .0000022f)) continue;
                    used.Add(candidate);
                    return candidate;
                }
            }
            throw new InvalidOperationException("No room for PIS-6 training contacts at " + site);
        }

        private static void ConfigureBench(SparkLevel98Process board, Circuit graph, Dictionary<string, Material> materials, List<string> csv)
        {
            board.standby = graph.Pin("12V_INPUT");
            board.bench = graph.SwitchedOutput("Guarded input disconnect", board.input, board.standby);
            board.inputDraw = graph.Load("Accelerated conversion input demand", board.standby, board.ground, 80f);
            SparkTerminal Regulated(string name, float voltage, float ohms, out SparkSwitch enable)
            {
                var raw = graph.Pin(name + " source");
                graph.Supply(name + " isolated training regulator", raw, board.ground, voltage);
                var output = graph.Pin(name);
                enable = graph.SwitchedOutput(name + " enable", raw, output);
                graph.Load(name + " bleed resistance", output, board.ground, ohms);
                return output;
            }

            board.mainRail = Regulated("5V_MAIN", 5f, 160f, out board.mainEnable);
            board.monitorRail = Regulated("3V3_MONITOR", 3.3f, 220f, out board.monitorEnable);
            var feed = Regulated("1V8_REGULATOR", 1.84f, 300f, out board.coreEnable);
            var impedance = graph.Load("Finite regulator output impedance", feed, board.actualRail, board.profile.feedOhms);
            board.actualRail.transform.SetParent(impedance.transform, true);
            graph.Load("Local rail bleed", board.actualRail, board.ground, 38f);
            var capacitorPad = graph.Pin("CP2.EFFECTIVE_PLATE");
            board.bulkEsr = graph.Load("CP2 polymer ESR", board.actualRail, capacitorPad, board.profile.goodEsr);
            board.bulk = graph.Cap("CP2 effective transient storage", capacitorPad, board.ground, board.profile.bulkFarads);
            var mlcc = graph.Pin("C5.BYPASS_PLATE");
            graph.Load("C5 local bypass impedance", board.actualRail, mlcc, .008f);
            graph.Cap("C5 local effective bypass", mlcc, board.ground, board.profile.bypassFarads);
            board.protectedFeed = graph.Pin("PROTECTED_LOAD_POS");
            board.workload = graph.SwitchedOutput("U9 independent electronic load gate", board.actualRail, board.protectedFeed);
            board.load = graph.Load("Programmable electronic load", board.protectedFeed, board.loadReturn, board.profile.idleOhms);

            Transform site(string id) => board.parts.Single(part => part.label == id).footprint;
            Vector3 a = graph.Root.InverseTransformPoint(site("U8").position) + new Vector3(-.004f, .0037f, -.0048f);
            Vector3 b = graph.Root.InverseTransformPoint(site("U9").position) + new Vector3(.0045f, .0037f, -.0048f);
            var sender = graph.Pin("U8.TRUSTED_LOAD_RAIL_VALID_OUT", a, true);
            var receiver = graph.Pin("U9.TRUSTED_LOAD_RAIL_VALID_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U8 trusted load validity transmitter", board.monitorRail, sender);
            var bias = graph.Load("U9 W075 receiver bias", receiver, board.ground, 10000f);
            receiver.transform.SetParent(bias.transform, true);
            board.jumper = new SparkLevel98Process.Route
            {
                id = "W075", group = "INDEPENDENT LOAD SAFETY", layer = 1, from = sender, to = receiver,
                copper = Copper(graph.Root, materials["ENIG_Gold"], "W075", a, b)
            };
            foreach (var terminal in new[] { sender, receiver, board.actualRail, board.sense, board.loadReturn })
            {
                var position = graph.Root.InverseTransformPoint(terminal.transform.position);
                Primitive(graph.Root, terminal.name + " contact", PrimitiveType.Cylinder, position - Vector3.up * .0004f,
                    new Vector3(.0024f, .00025f, .0024f), materials["ENIG_Gold"]);
            }
            csv.Add(Row(board.jumper, a, b));

            board.locationMarker = Marker(graph.Root, "Footprint locator", materials["LED_Green"], .0054f);
            board.startMarker = Marker(graph.Root, "Start contact locator", materials["ENIG_Gold"], .0028f);
            board.endMarker = Marker(graph.Root, "End contact locator", materials["LED_Green"], .0028f);
            board.statusLamp = Primitive(graph.Root, "U9 protected load lamp", PrimitiveType.Cube,
                b + new Vector3(.003f, .0008f, 0), new Vector3(.0017f, .0005f, .0012f), materials["LED_Green"]);
            BoardText(graph.Root, "PIS-6  /  TRANSIENT SHIELD", new Vector3(-.058f, .00245f, -.052f), .00060f, new Color(.80f, .89f, .86f));
            WorkLight(graph.Root, "Warm surface key", new Vector3(-.045f, .055f, -.029f), new Color(1f, .89f, .72f), 5.2f);
            WorkLight(graph.Root, "Cool metrology fill", new Vector3(.054f, .041f, .028f), new Color(.65f, .87f, 1f), 4.5f);
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
            line.widthMultiplier = name == "W075" ? .041f : .023f;
            line.positionCount = 3;
            line.SetPosition(0, a);
            line.SetPosition(1, new Vector3(b.x, Mathf.Max(a.y, b.y) + (name == "W075" ? .005f : 0f), a.z));
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
                    ("currentLimit", 5f), ("state", SparkPowerSupplyState.Active));
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
