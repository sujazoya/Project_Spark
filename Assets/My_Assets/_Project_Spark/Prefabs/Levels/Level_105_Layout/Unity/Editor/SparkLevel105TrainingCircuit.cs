using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.ExpansionNexus;
using UnityEngine;

namespace ProjectSpark.EditorTools.ExpansionNexus
{
    internal static partial class SparkLevel105Builder
    {
        private static void ConfigureGraph(SparkLevel105Process board, Circuit graph, string[][] plans, IEnumerable<string> viaIds,
            Dictionary<string, Material> materials, List<string> csv)
        {
            Vector3 Site(string id) => graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == id).footprint.position);
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 12f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity reference", board.input, board.ground, 10000);
            board.standby = graph.Pin("ENX13.PROTECTED_12V");
            board.bench = graph.SwitchedOutput("Protected 12 V input", board.input, board.standby);
            graph.Load("12 V bench witness", board.standby, board.ground, 1000f);
            var regulator = graph.Pin("ENX13.AUX_RAW");
            graph.Supply("ENX-13 3.3 V training regulator", regulator, board.ground, 3.3f);
            board.aux = graph.Pin("ENX13.AUX_3V3");
            board.auxGate = graph.SwitchedOutput("Auxiliary enable", regulator, board.aux);
            graph.Load("3.3 V supply witness", board.aux, board.ground, 1000f);
            board.cardGround = graph.Pin("ECX4.GND");
            board.presence = graph.Pin("ECX4.PRSNT");
            board.endpointPower = graph.Pin("ECX4.EDGE_3V3");
            board.groundContact = graph.SwitchedOutput("J1 ground-first contact", board.ground, board.cardGround);
            board.presenceContact = graph.SwitchedOutput("J1 presence contact", board.aux, board.presence);
            graph.Load("Presence witness", board.presence, board.cardGround, 10000);
            board.powerContact = graph.SwitchedOutput("J1 auxiliary power contact", board.aux, board.endpointPower);
            graph.Load("Endpoint power load", board.endpointPower, board.cardGround, 330);
            var resetDrive = graph.Pin("J1.RESET_RELEASE");
            board.resetGate = graph.SwitchedOutput("Root reset state", board.aux, resetDrive);
            board.resetSense = graph.Pin("ECX4.RESET_RELEASE");
            var resetContact = graph.SwitchedOutput("J1 reset sideband contact", resetDrive, board.resetSense);
            graph.Load("Endpoint reset pull-down", board.resetSense, board.cardGround, 10000);

            var slot = Site("J1");
            board.seatPosition = slot + Vector3.up * .024f;
            var contacts = new List<SparkSwitch> { resetContact };
            board.lanes = Enumerable.Range(0, 4).Select(_ => new SparkLevel105Process.Lane()).ToArray();
            var groups = plans.GroupBy(row => row[1]).ToArray();
            var routes = new List<SparkLevel105Process.Route>();
            var witnesses = new List<SparkTerminal>();
            var remaining = new Queue<string>(viaIds);
            var pads = new List<Vector3>();
            var sites = board.parts.Where(part => !part.onCard).Select(part => Site(part.label)).ToArray();
            foreach (var group in groups)
            {
                int viasInGroup = group.Key is "PowerGround" or "ClockReset" or "Connector" ? 6 : 5;
                SparkTerminal previous = board.input;
                foreach (var task in group)
                {
                    string id = task[0];
                    int index = routes.Count;
                    int leg = index is >= 40 and < 48 ? index - 40 : index is >= 64 and < 72 ? index - 64 + 8 : -1;
                    int lane = leg / 4, signal = leg % 4;
                    bool clock = index is 18 or 19;
                    bool functional = clock || leg >= 0;
                    string net = clock ? index == 18 ? "REFCLK_P" : "REFCLK_N" :
                        leg >= 0 ? $"LANE{lane}_{(signal < 2 ? "TX" : "RX")}_{(signal % 2 == 0 ? "P" : "N")}" : $"{group.Key}_CHECK";
                    Vector3 site = sites[index % sites.Length];
                    Vector3 entry = FindPad(functional ? Site(clock ? "U3" : "U1") : site, index % 8, pads);
                    Vector3 destination = FindPad(functional ? slot + new Vector3(-.029f + (index % 8) * .0076f, 0f,
                        clock ? -.014f : lane < 2 ? .013f : .021f) : site, (index + 2) % 8, pads);
                    var a = graph.Pin(functional ? net + ".ROOT" : id + ".A", entry, true);
                    var b = graph.Pin(functional ? net + ".J1_SLOT" : id + ".B", destination, true);
                    var route = new SparkLevel105Process.Route
                    {
                        id = id, group = group.Key, net = net, entry = entry, destination = destination,
                        layer = functional ? 1 : index % 8 + 1, from = a, to = b,
                        copper = Copper(graph.Root, materials[functional ? "gold" : "copper"], id, entry, destination)
                    };
                    var assemblyOut = functional ? graph.Pin(id + ".ASSEMBLY_CHECK") : a;
                    var contact = graph.SwitchedOutput(id + " solder witness", previous, assemblyOut, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = functional ? assemblyOut : b;
                    if (functional)
                    {
                        bool reverse = leg >= 0 && signal >= 2;
                        var edge = graph.Pin("ECX4.EDGE_" + net);
                        edge.transform.SetParent(board.card, false);
                        edge.transform.localPosition = new Vector3(-.032f + (clock ? index - 2 : leg) * .0037f, .0036f, -.0164f);
                        var receiver = reverse ? a : edge;
                        SparkTerminal slotPin = b;
                        if (leg == 8)
                        {
                            slotPin = graph.Pin("C22.LANE2_TX_P_OUT");
                            board.coupling = graph.SwitchedOutput("C22 lane 2 series coupling contact", b, slotPin, true);
                        }
                        if (index == 19)
                        {
                            slotPin = graph.Pin("U3.REFCLK_N_BUFFER_OUT");
                            board.clockBuffer = graph.SwitchedOutput("U3 negative clock leg buffer", b, slotPin, true);
                        }
                        SparkSwitch driver = reverse
                            ? graph.SwitchedOutput(net + " endpoint driver", board.endpointPower, edge)
                            : graph.SwitchedOutput(net + " root driver", board.aux, a);
                        SparkSwitch bridge = reverse ? graph.SwitchedOutput("J1 contact " + net, edge, slotPin)
                            : graph.Contact("J1 contact " + net, slotPin, edge);
                        if (!reverse)
                        {
                            // Preserve card-relative placement while attaching the pin to a solved device.
                            var edgeDevice = graph.Load(net + " ECX-4 receiver bias", receiver, board.cardGround, 10000);
                            edgeDevice.transform.SetParent(board.card, false);
                            receiver.transform.SetParent(edgeDevice.transform, true);
                        }
                        else
                        {
                            driver.transform.SetParent(board.card, false);
                            edge.transform.localPosition = new Vector3(-.032f + leg * .0037f, .0036f, -.0164f);
                            var load = graph.Load(net + " root receiver bias", receiver, board.ground, 10000);
                            receiver.transform.SetParent(load.transform, true);
                        }
                        contacts.Add(bridge);
                        if (clock)
                        {
                            if (index == 18) { board.clockP = receiver; board.clockDriveP = driver; }
                            else { board.clockN = receiver; board.clockDriveN = driver; }
                        }
                        else
                        {
                            board.lanes[lane].drive[signal] = driver;
                            board.lanes[lane].sense[signal] = receiver;
                            board.lanes[lane].lengthMm[signal] = (Mathf.Abs(entry.x - destination.x) +
                                Mathf.Abs(entry.z - destination.z)) * 1000f;
                            if (leg == 14) board.intermittentContact = bridge;
                        }
                    }
                    else if (viasInGroup-- > 0)
                    {
                        route.viaId = remaining.Dequeue();
                        route.layer = index % 7 + 1;
                        route.exitLayer = 8;
                        previous = graph.Pin(route.viaId + ".L08", destination);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", b, previous);
                    }
                    routes.Add(route);
                    csv.Add(Row(route));
                }
                graph.Load(group.Key + " continuity witness", previous, board.ground, 1000);
                witnesses.Add(previous);
            }
            if (routes.Count != SparkLevel105Process.RouteCount || remaining.Count != 0 || witnesses.Count != 7 ||
                routes.Select(route => route.layer).Distinct().Count() != 8 || board.parts.Any(part => !part.contact) ||
                board.lanes.Any(lane => lane.sense.Any(pin => !pin)))
                throw new InvalidOperationException("Incomplete ENX-13 training assignment.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
            board.slotContacts = contacts.ToArray();
            ConfigureSafety(board, graph, materials, csv, Site("U12"), Site("U13"));

            board.couplingPart = Array.FindIndex(board.parts, part => part.label == "C22");
            board.clockPart = Array.FindIndex(board.parts, part => part.label == "U3");
            board.slotPart = Array.FindIndex(board.parts, part => part.label == "J1");
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["red"], .0049f);
            board.startMarker = Marker(graph.Root, "Source locator", materials["gold"], .0026f);
            board.endMarker = Marker(graph.Root, "Destination locator", materials["green"], .0026f);
            var faultLocations = new[] { Site("C22"), Site("U3"), slot + new Vector3(.035f, .005f, -.009f) };
            board.faultOverlays = faultLocations.Select((position, index) =>
            {
                var marker = Marker(graph.Root, "Measured fault " + (index + 1), materials["red"], .005f);
                marker.transform.localPosition = position + Vector3.up * .004f;
                marker.enabled = true;
                marker.gameObject.SetActive(false);
                return marker.gameObject;
            }).ToArray();
            BoardText(graph.Root, "ENX-13  /  EXPANSION NEXUS", new Vector3(-.079f, .0038f, -.082f), .00078f, Color.white);
            BoardText(graph.Root, "J1  /  ECX-4", slot + new Vector3(0f, .0038f, -.017f), .00068f, new Color(.76f, .89f, 1f));
            BoardText(board.card, "ECX-4", new Vector3(.022f, .0044f, .009f), .00087f, Color.white);
            WorkLight(graph.Root, "Bench key", new Vector3(-.072f, .073f, -.038f), new Color(1f, .91f, .80f), 10.2f);
            WorkLight(graph.Root, "Connector fill", new Vector3(.073f, .075f, .045f), new Color(.69f, .87f, 1f), 10.0f);
        }

        private static void ConfigureSafety(SparkLevel105Process board, Circuit graph, Dictionary<string, Material> materials,
            List<string> csv, Vector3 u12, Vector3 u13)
        {
            var a = u12 + new Vector3(-.004f, .0032f, -.010f);
            var b = u13 + new Vector3(.004f, .0032f, -.010f);
            var sender = graph.Pin("U12.TRUSTED_EXPANSION_READY_OUT", a, true);
            var receiver = graph.Pin("U13.TRUSTED_EXPANSION_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U12 validity pulse", board.aux, sender);
            var bias = graph.Load("U13 feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(bias.transform, true);
            board.jumper = new SparkLevel105Process.Route
            {
                id = "W133", net = "TRUSTED_EXPANSION_READY", group = "Independent safety", layer = 1,
                from = sender, to = receiver, entry = a, destination = b, copper = Copper(graph.Root, materials["gold"], "W133", a, b)
            };
            csv.Add(Row(board.jumper));
            foreach (var point in new[] { a, b })
                Primitive(graph.Root, "W133 solder pad", PrimitiveType.Cylinder, point, new Vector3(.0028f, .00027f, .0028f), materials["gold"]);
            board.monitorSupply = graph.Pin("U13.INDEPENDENT_3V3");
            var feed = graph.Load("Independent monitor supply", board.aux, board.monitorSupply, 1f);
            board.monitorSupply.transform.SetParent(feed.transform, true);
            graph.Load("Monitor power witness", board.monitorSupply, board.ground, 1000f);
            board.protectedFeed = graph.Pin("U13.PROTECTED_WORKLOAD");
            board.workload = graph.SwitchedOutput("U13 independent permit", board.monitorSupply, board.protectedFeed);
            graph.Load("Protected workload witness", board.protectedFeed, board.ground, 330f);
            board.statusLamp = Primitive(graph.Root, "U13 permit lamp", PrimitiveType.Cube, b + new Vector3(.007f, .001f, 0),
                new Vector3(.0025f, .0010f, .0018f), materials["green"]);
            BoardText(graph.Root, "W133  /  U12 → U13", (a + b) * .5f + new Vector3(0, .0009f, -.005f), .00053f, Color.white);
        }

        private static Vector3 FindPad(Vector3 site, int start, List<Vector3> used)
        {
            for (int ring = 0; ring < 8; ring++)
            for (int step = 0; step < 8; step++)
            {
                float angle = (start + step) * Mathf.PI / 4f;
                float radius = .0044f + .0022f * ring;
                var point = new Vector3(site.x + radius * Mathf.Cos(angle), .004f, site.z + radius * Mathf.Sin(angle));
                if (Mathf.Abs(point.x) > .130f || Mathf.Abs(point.z) > .091f ||
                    used.Exists(other => (other - point).sqrMagnitude < .0000025f)) continue;
                used.Add(point);
                return point;
            }
            throw new InvalidOperationException("No space for an ENX-13 training pad near " + site);
        }
    }
}
