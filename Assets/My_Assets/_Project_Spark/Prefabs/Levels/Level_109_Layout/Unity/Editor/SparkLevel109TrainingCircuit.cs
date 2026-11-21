using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.ThermalEndurance;
using UnityEngine;

namespace ProjectSpark.EditorTools.ThermalEndurance
{
    internal static partial class SparkLevel109Builder
    {
        private static List<string> ConfigureGraph(SparkLevel109Process board, Circuit graph, string[][] plans,
            IEnumerable<string> viaIds, Dictionary<string, Material> mats)
        {
            Vector3 Site(string label) => graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == label).footprint.position);
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 5f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity reference", board.input, board.ground, 10000f);
            board.standby = graph.Pin("TRX17.PROTECTED_5V");
            board.bench = graph.SwitchedOutput("Bench isolation", board.input, board.standby);
            graph.Load("Protected rail load", board.standby, board.ground, 1000f);
            (board.aux, board.auxGate) = Regulate("TRX17.AUX_3V3", 3.3f);
            (board.cpuRail, board.cpuGate) = Regulate("U1.CORE_1V05", 1.05f);
            (board.ddrRail, board.ddrGate) = Regulate("DDR4.VDD_1V2", 1.2f);
            board.cpuLoad = graph.Load("U1 profile load", board.cpuRail, board.ground, 3.15f);
            board.memoryA = graph.Load("U2 profile load", board.ddrRail, board.ground, 3.2f);
            board.memoryB = graph.Load("U3 profile load", board.ddrRail, board.ground, 3.1f);
            board.fanSense = graph.Pin("U5.FAN_RESPONSE");
            board.fanDrive = graph.SwitchedOutput("U5 fan control and tach witness", board.aux, board.fanSense);
            graph.Load("Fan response input load", board.fanSense, board.ground, 10000f);
            board.phases = Enumerable.Range(0, 4).Select(_ => new SparkLevel109Process.Phase()).ToArray();

            var routes = new List<SparkLevel109Process.Route>(SparkLevel109Process.RouteCount);
            var witnesses = new List<SparkTerminal>();
            var remaining = new Queue<string>(viaIds);
            var pads = new List<Vector3>();
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var csv = new List<string> { "task,domain,training_net,entry_layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                int quota = witnesses.Count < 5 ? 8 : 7;
                var previous = board.input;
                foreach (var row in group)
                {
                    int i = routes.Count;
                    int phase = i is >= 22 and < 26 ? i - 22 : -1;
                    bool ddrContact = i == 46, pcieCheck = i == 94;
                    bool functional = phase >= 0 || ddrContact || pcieCheck;
                    string net = phase >= 0 ? $"PHASE_{phase + 1}_INPUT" : ddrContact ? "U3_DIAGNOSTIC_RX" :
                        pcieCheck ? "PCIE_TRAINING_STABILITY" : "CHECK_" + (witnesses.Count + 1);
                    string pinOwner = phase >= 0 ? $"Q{phase * 2 + 1:00}" : ddrContact ? "U3" : "U1";
                    Vector3 near = functional ? Site(pinOwner) : sites[i % sites.Length];
                    var start = FindPad(near, i % 8, pads);
                    var end = FindPad(near, (i + 2) % 8, pads);
                    var a = graph.Pin(functional ? pinOwner + "." + net + ".OUT" : row[0] + ".A", start, true);
                    var b = graph.Pin(functional ? pinOwner + "." + net + ".IN" : row[0] + ".B", end, true);
                    var route = new SparkLevel109Process.Route
                    {
                        id = row[0], group = group.Key, net = net, from = a, to = b, entry = start, destination = end,
                        layer = functional ? phase >= 0 ? 2 : ddrContact ? 4 : 7 : i % SparkLevel109Process.LayerCount + 1,
                        copper = Copper(graph.Root, mats[functional ? "GOLD" : "COPPER"], row[0], start, end)
                    };
                    var installedContact = functional ? graph.Pin(row[0] + ".ASSEMBLY_CHECK") : a;
                    var solder = graph.SwitchedOutput(row[0] + " solder witness", previous, installedContact, i >= board.parts.Length);
                    if (i < board.parts.Length) board.parts[i].contact = solder;
                    previous = functional ? installedContact : b;
                    if (phase >= 0)
                    {
                        graph.SwitchedOutput(net + " branch", board.standby, a, true);
                        var loadNode = graph.Pin(pinOwner + ".PHASE_CURRENT");
                        var fet = graph.Load(pinOwner + " conduction loss", b, loadNode, .18f);
                        loadNode.transform.SetParent(fet.transform, true);
                        board.phases[phase].mosfet = fet;
                        board.phases[phase].load = graph.Load($"Phase {phase + 1} work demand", loadNode, board.ground, 23f);
                    }
                    else if (ddrContact)
                    {
                        board.memoryContact = graph.SwitchedOutput("U3 temperature-sensitive BGA contact", board.ddrRail, a, true);
                        board.memorySense = b;
                        graph.Load("U3 receive termination witness", b, board.ground, 10000f);
                    }
                    else if (pcieCheck)
                    {
                        graph.SwitchedOutput("PCIe auxiliary diagnostic drive", board.aux, a, true);
                        board.pcieSense = b;
                        graph.Load("PCIe stability witness", b, board.ground, 10000f);
                    }
                    else if (quota > 0)
                    {
                        quota--;
                        route.viaId = remaining.Dequeue();
                        route.layer = i % 9 + 1;
                        route.exitLayer = route.layer + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", b, previous);
                    }
                    routes.Add(route);
                    csv.Add(Row(route));
                }
                graph.Load(group.Key + " continuity witness", previous, board.ground, 1000f);
                witnesses.Add(previous);
            }
            if (remaining.Count != 0 || routes.Count != SparkLevel109Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel109Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel109Process.LayerCount ||
                board.parts.Any(part => !part.contact) || board.phases.Any(phase => !phase.mosfet) || !board.memoryContact)
                throw new InvalidOperationException("Incomplete TRX-17 training routes, vias or test paths.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();

            Vector3 from = Site("U18") + new Vector3(-.005f, .0018f, -.009f);
            Vector3 to = Site("U19") + new Vector3(.006f, .0018f, -.009f);
            var sender = graph.Pin("U18.TRUSTED_THERMAL_HEALTH_OUT", from, true);
            var receiver = graph.Pin("U19.TRUSTED_THERMAL_HEALTH_IN", to, true);
            board.heartbeat = graph.SwitchedOutput("U18 thermal health heartbeat", board.aux, sender);
            var bias = graph.Load("U19 feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(bias.transform, true);
            board.jumper = new SparkLevel109Process.Route
            {
                id = "W165", group = "Independent safety", net = "TRUSTED_THERMAL_HEALTH", layer = 1,
                from = sender, to = receiver, entry = from, destination = to, copper = Copper(graph.Root, mats["GOLD"], "W165", from, to)
            };
            csv.Add(Row(board.jumper));
            board.monitorSupply = graph.Pin("U19.INDEPENDENT_3V3");
            var feed = graph.Load("U19 independent supply", board.aux, board.monitorSupply, 1f);
            board.monitorSupply.transform.SetParent(feed.transform, true);
            graph.Load("U19 supply witness", board.monitorSupply, board.ground, 1000f);
            board.protectedFeed = graph.Pin("U19.PROTECTED_PEAK_WORKLOAD");
            board.workload = graph.SwitchedOutput("U19 protected peak relay", board.monitorSupply, board.protectedFeed);
            graph.Load("Peak authorizer load", board.protectedFeed, board.ground, 330f);
            foreach (Vector3 point in new[] { from, to })
                Primitive(graph.Root, "W165 contact", PrimitiveType.Cylinder, point, new Vector3(.003f, .00025f, .003f), mats["GOLD"]);

            board.startMarker = Marker(graph.Root, "Copper source locator", mats["GOLD"], .003f);
            board.endMarker = Marker(graph.Root, "Copper destination locator", mats["CYAN"], .003f);
            board.locationMarker = Marker(graph.Root, "Placement locator", mats["CYAN"], .0049f);
            board.statusLamp = Primitive(graph.Root, "U19 protected permit lamp", PrimitiveType.Cube, to + new Vector3(.003f, .0007f, .006f),
                new Vector3(.0026f, .00075f, .0018f), mats["GREEN"]);
            board.faultOverlays = new[] { Site("Q05"), Site("U1"), Site("U3") }.Select((point, index) =>
            {
                var locator = Marker(graph.Root, "Measured fault " + (index + 1), mats["RED"], .0058f);
                locator.transform.localPosition = point + Vector3.up * .010f;
                locator.enabled = true;
                locator.gameObject.SetActive(false);
                return locator.gameObject;
            }).ToArray();
            board.thermalMarkers = new[] { Site("U1"), Site("Q01"), Site("Q03"), Site("Q05"), Site("Q07"), Site("U3") }
                .Select((point, index) =>
                {
                    var marker = Marker(graph.Root, "Thermal sample " + (index + 1), mats["CYAN"], index == 0 ? .012f : .007f);
                    marker.transform.localPosition = point + Vector3.up * (index == 0 ? .015f : .009f);
                    return marker;
                }).ToArray();
            BoardText(graph.Root, "TRX-17  /  THERMAL ENDURANCE", new Vector3(-.095f, .0035f, -.095f), .0010f, Color.white);
            BoardText(graph.Root, "W165  /  U18 → U19", (from + to) * .5f + Vector3.forward * .0056f, .00062f, Color.white);
            BoardText(graph.Root, "CPU  /  TIM", Site("U1") + new Vector3(0, .0035f, -.032f), .00066f, Color.white);
            BoardText(graph.Root, "PHASE 3", Site("Q05") + new Vector3(.004f, .0035f, -.013f), .00057f, Color.white);
            WorkLight(graph.Root, "Warm key", new Vector3(-.076f, .082f, -.042f), new Color(1f, .90f, .79f), 10.5f);
            WorkLight(graph.Root, "Cool fill", new Vector3(.078f, .091f, .052f), new Color(.66f, .86f, 1f), 10.3f);
            return csv;

            (SparkTerminal, SparkSwitch) Regulate(string name, float voltage)
            {
                var source = graph.Pin(name + ".SUPPLY");
                var supply = graph.Supply(name + " training regulator", source, board.ground, voltage);
                if (voltage == 1.2f) Set(supply, ("currentLimit", 8f));
                var terminal = graph.Pin(name);
                var contact = graph.SwitchedOutput(name + " enable", source, terminal);
                graph.Load(name + " witness", terminal, board.ground, 1000f);
                return (terminal, contact);
            }
        }

        private static string Row(SparkLevel109Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 site, int offset, List<Vector3> used)
        {
            for (int ring = 0; ring < 9; ring++)
            for (int step = 0; step < 8; step++)
            {
                float angle = (offset + step) * Mathf.PI / 4f;
                float radius = .0046f + .0022f * ring;
                var point = new Vector3(site.x + radius * Mathf.Cos(angle), .0043f, site.z + radius * Mathf.Sin(angle));
                if (Mathf.Abs(point.x) > .147f || Mathf.Abs(point.z) > .107f ||
                    used.Exists(last => (point - last).sqrMagnitude < .0000027f)) continue;
                used.Add(point);
                return point;
            }
            throw new InvalidOperationException("No free TRX-17 terminal near " + site);
        }
    }
}
