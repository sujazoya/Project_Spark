using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.AutoLevelGenerator;
using ProjectSpark.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    internal static partial class SparkLevel106Builder
    {
        private static void ConfigureBoard(SparkLevel106Process board, SparkAutoLevelBlueprint blueprint, Circuit graph,
            Dictionary<string, Material> materials, List<string> csv)
        {
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 12f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Source reference", board.input, board.ground, 10000f);
            var groups = blueprint.Routes.GroupBy(route => route.Subsystem).ToArray();
            var routes = new List<SparkLevel106Process.Route>();
            var witnesses = new List<SparkTerminal>();
            var used = new List<Vector3>();
            var kit = board.parts.Select(part => graph.Root.InverseTransformPoint(part.footprint.position)).ToArray();
            int via = 0;
            for (int group = 0; group < groups.Length; group++)
            {
                int inGroup = 0;
                SparkTerminal previous = board.input;
                foreach (var task in groups[group])
                {
                    int index = routes.Count;
                    int site = index % kit.Length;
                    var footprint = kit[site];
                    float clearRadius = site == 0 ? .023f : site is 1 or 2 ? .0085f : .0048f;
                    Vector3 start = FindPad(footprint, clearRadius, index >= kit.Length ? 3 : 0, used);
                    Vector3 end = FindPad(footprint, clearRadius, index >= kit.Length ? 5 : 2, used);
                    var a = graph.Pin(task.Id + ".A", start, true);
                    var b = graph.Pin(task.Id + ".B", end, true);
                    var route = new SparkLevel106Process.Route
                    {
                        id = task.Id, group = groups[group].Key, layer = (index * 3 + group) % 10 + 1,
                        from = a, to = b, copper = Copper(graph.Root, materials["COPPER"], task.Id, start, end)
                    };
                    var contact = graph.SwitchedOutput(task.Id + " series contact", previous, a, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = b;
                    if (inGroup < 6)
                    {
                        route.viaId = blueprint.Vias[via++].Id;
                        route.exitLayer = (route.layer + 2) % 10 + 1;
                        previous = graph.Pin(route.viaId + ".L" + route.exitLayer, end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " training barrel", b, previous);
                        b.transform.SetParent(route.barrel.transform, true);
                    }
                    routes.Add(route);
                    csv.Add(Row(route, start, end));
                    inGroup++;
                }
                graph.Load(groups[group].Key + " continuity witness", previous, board.ground, 1000f);
                witnesses.Add(previous);
            }
            if (via != 42 || routes.Count != 140 || routes.Select(route => route.layer).Distinct().Count() != 10)
                throw new InvalidOperationException("The training circuit expected seven domains and six vias per domain.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
        }

        private static Vector3 FindPad(Vector3 site, float clearRadius, int corner, List<Vector3> used)
        {
            // Fictional surface contacts sit beside each source footprint.
            for (int ring = 0; ring < 10; ring++)
            {
                for (int attempt = 0; attempt < 8; attempt++)
                {
                    float angle = (corner + attempt) * Mathf.PI / 4f;
                    float radius = clearRadius + ring * .003f;
                    var candidate = new Vector3(site.x + radius * Mathf.Cos(angle), .0027f, site.z + radius * Mathf.Sin(angle));
                    if (Mathf.Abs(candidate.x) > .137f || Mathf.Abs(candidate.z) > .098f ||
                        used.Exists(point => (point - candidate).sqrMagnitude < .000003f)) continue;
                    used.Add(candidate);
                    return candidate;
                }
            }
            throw new InvalidOperationException("No space for training contacts at " + site);
        }

        private static void ConfigureBoot(SparkLevel106Process board, Circuit graph, Dictionary<string, Material> materials,
            List<string> csv)
        {
            board.standby = graph.Pin("12V_STANDBY");
            board.bench = graph.SwitchedOutput("Bench disconnect", board.input, board.standby);
            graph.Load("Standby witness", board.standby, board.ground, 470f);
            SparkTerminal Regulated(string name, float voltage, out SparkSwitch enable)
            {
                var ideal = graph.Pin(name + " source");
                graph.Supply(name + " training regulator", ideal, board.ground, voltage);
                var output = graph.Pin(name);
                enable = graph.SwitchedOutput(name + " enable", ideal, output);
                graph.Load(name + " operating load", output, board.ground, 200f);
                return output;
            }
            board.core = Regulated("CPU_VCORE", 1f, out board.coreEnable);
            board.ddr = Regulated("DDR_VDD", 1.2f, out board.ddrEnable);
            var logic = graph.Pin("3V3_AUX");
            graph.Supply("Auxiliary training regulator", logic, board.ground, 3.3f);

            board.resetSense = graph.Pin("CPU_RESET_N");
            board.resetSignal = graph.SwitchedOutput("Power-good reset driver", logic, board.resetSense);
            graph.Load("Reset bias", board.resetSense, board.ground, 1000f);
            board.flashSense = graph.Pin("U6_SPI_CONTACT");
            var series = graph.Load("U6 SPI source impedance", board.resetSense, board.flashSense, 47f);
            board.flashSense.transform.SetParent(series.transform, true);
            var bridge = graph.Pin("U6_SOLDER_BRIDGE");
            board.flashBridge = graph.SwitchedOutput("U6 fault-controlled solder bridge", board.flashSense, bridge);
            graph.Load("Bridge impedance", bridge, board.ground, 2f);
            graph.Load("SPI input bias", board.flashSense, board.ground, 10000f);
            board.linkSense = graph.Pin("PCIE_X4_WITNESS");
            graph.SwitchedOutput("PCIe diagnostic endpoint", board.resetSense, board.linkSense, true);
            graph.Load("PCIe endpoint bias", board.linkSense, board.ground, 1000f);
            board.storageSense = graph.Pin("U7_EMMC_CONTACT");
            board.storageContact = graph.SwitchedOutput("U7 boot media contact", board.resetSense, board.storageSense, true);
            graph.Load("eMMC input bias", board.storageSense, board.ground, 1000f);

            int Part(string reference) => Array.FindIndex(board.parts, part => part.label == reference);
            board.repairParts = new[] { Part("RES6"), Part("U6"), Part("U7") };
            int supervisor = Part("U15");
            if (supervisor < 0 || board.repairParts.Any(part => part < 0))
                throw new InvalidOperationException("Original repair or U15 layout slots are missing.");

            var module = Child(graph.Root, "U14 fictional System Boot Monitor", new Vector3(.122f, .006f, .061f));
            Primitive(module, "Module substrate", PrimitiveType.Cube, new Vector3(0, .0005f, 0),
                new Vector3(.022f, .001f, .014f), materials["PCB"]);
            Primitive(module, "Monitor IC", PrimitiveType.Cube, new Vector3(-.004f, .0018f, 0),
                new Vector3(.009f, .002f, .009f), materials["DARK_METAL"]);
            BoardText(module, "U14", new Vector3(-.004f, .003f, 0), .00046f, Color.white);
            board.statusLamp = Primitive(module, "U14 status", PrimitiveType.Cube, new Vector3(-.008f, .0016f, .0049f),
                new Vector3(.003f, .0014f, .0016f), materials["CYAN"]);
            Vector3 a = module.localPosition + new Vector3(.005f, .002f, -.003f);
            Vector3 b = graph.Root.InverseTransformPoint(board.parts[supervisor].footprint.position) + new Vector3(-.003f, .006f, .005f);
            var sender = graph.Pin("U14.TRAINING_READY_OUT", a, true);
            var receiver = graph.Pin("U15.TRAINING_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U14 heartbeat transmitter", logic, sender);
            var bias = graph.Load("U15 receiver bias", receiver, board.ground, 10000f);
            receiver.transform.SetParent(bias.transform, true);
            board.jumper = new SparkLevel106Process.Route
            {
                id = "W141", group = "INDEPENDENT SAFETY", layer = 1, from = sender, to = receiver,
                copper = Copper(graph.Root, materials["GOLD"], "W141", a, b)
            };
            Primitive(graph.Root, "U14 service socket", PrimitiveType.Cylinder, a - Vector3.up * .00045f,
                new Vector3(.0034f, .00045f, .0034f), materials["GOLD"]);
            Primitive(graph.Root, "U15 service socket", PrimitiveType.Cylinder, b - Vector3.up * .00045f,
                new Vector3(.0034f, .00045f, .0034f), materials["GOLD"]);
            csv.Add(Row(board.jumper, a, b));
            board.protectedFeed = graph.Pin("PROTECTED_BOOT_LOAD");
            board.workload = graph.SwitchedOutput("U15 independent load relay", board.standby, board.protectedFeed);
            graph.Load("Protected boot workload", board.protectedFeed, board.ground, 1000f);
            board.locationMarker = Marker(graph.Root, "Footprint locator", materials["CYAN"], .009f);
            board.startMarker = Marker(graph.Root, "Start contact locator", materials["GOLD"], .004f);
            board.endMarker = Marker(graph.Root, "End contact locator", materials["CYAN"], .004f);
            WorkLight(graph.Root, "Warm bench key", new Vector3(-.064f, .075f, -.038f), new Color(1f, .88f, .70f), 5.8f);
            WorkLight(graph.Root, "Cool bench fill", new Vector3(.082f, .063f, .044f), new Color(.66f, .87f, 1f), 4.3f);
            BoardText(graph.Root, "SFX-14  /  FIRST BOOT", new Vector3(-.084f, .0014f, -.092f), .0011f, new Color(.82f, .87f, .84f));
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
            line.widthMultiplier = name == "W141" ? .041f : .023f;
            line.positionCount = 3;
            line.SetPosition(0, a);
            line.SetPosition(1, new Vector3(b.x, Mathf.Max(a.y, b.y) + (name == "W141" ? .005f : 0f), a.z));
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
                    ("currentLimit", 2f), ("state", SparkPowerSupplyState.Active));
                positive.transform.SetParent(device.transform, true);
                Set(positive, ("owner", device));
                return device;
            }

            public SparkSwitch SwitchedOutput(string name, SparkTerminal a, SparkTerminal b, bool closed = false)
            {
                var contact = Child(Root, name).gameObject.AddComponent<SparkSwitch>();
                Set(contact, ("inputTerminal", a), ("outputTerminal", b), ("closedResistance", .001f),
                    ("state", closed ? SparkSwitchState.Closed : SparkSwitchState.Open));
                b.transform.SetParent(contact.transform, true);
                return contact;
            }

            public SparkResistor Load(string name, SparkTerminal a, SparkTerminal b, float ohms)
            {
                var resistor = Child(Root, name).gameObject.AddComponent<SparkResistor>();
                Set(resistor, ("terminalA", a), ("terminalB", b), ("resistance", ohms), ("maximumPowerWatts", 12f));
                Set(resistor.GetComponent<SparkResistorBurn>(), ("ratedWatts", 12f),
                    ("thermalCapacityJPerC", .8f), ("coolingWattsPerC", .35f));
                return resistor;
            }
        }
    }
}
