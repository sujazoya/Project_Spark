using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.SignalCrucible;
using UnityEngine;

namespace ProjectSpark.EditorTools.SignalCrucible
{
    internal static partial class SparkLevel111Builder
    {
        private const string ReturnTask = "W073";
        private const string ReturnVia = "V37";

        private static List<string> ConfigureGraph(SparkLevel111Process board, Circuit graph, string[][] plans, IEnumerable<string> viaIds,
            Dictionary<string, Material> mats)
        {
            Vector3 Site(string id) => graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == id).footprint.position);
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 5f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Bench continuity reference", board.input, board.ground, 10000f);
            board.standby = graph.Pin("SIX19.PROTECTED_5V");
            board.bench = graph.SwitchedOutput("Protected bench input", board.input, board.standby);
            graph.Load("Protected bench witness", board.standby, board.ground, 1000f);
            (board.aux, board.auxGate) = Regulate("AUX_3V3", 3.3f);
            (board.retimerRail, board.retimerGate) = Regulate("RETIMER_1V8", 1.8f);
            board.returnSense = graph.Pin("U8.LANE2_REFERENCE");
            board.heatLoad = graph.Pin("U8.ACTIVE_LOAD");
            board.loadGate = graph.SwitchedOutput("U8 load activity", board.retimerRail, board.heatLoad);
            graph.Load("U8 active dissipation witness", board.heatLoad, board.ground, 110f);
            board.equalizerBias = graph.Pin("U8.CTLE_BIAS");
            var biasFeed = graph.Load("U8 equalizer bias feed", board.retimerRail, board.equalizerBias, 100f);
            board.equalizerBias.transform.SetParent(biasFeed.transform, true);
            graph.Load("U8 bias reference", board.equalizerBias, board.ground, 10000f);
            board.thermalShunt = Shunt("U8 hot equalizer drift", board.equalizerBias, 125f);

            board.lanes = Enumerable.Range(0, 4).Select(_ => new SparkLevel111Process.Lane()).ToArray();
            var routes = new List<SparkLevel111Process.Route>(SparkLevel111Process.RouteCount);
            var witnesses = new List<SparkTerminal>();
            var pads = new List<Vector3>();
            var remaining = new Queue<string>(viaIds.Where(id => id != ReturnVia));
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var csv = new List<string> { "task,domain,training_net,entry_layer,source_pad,destination_pad,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                int quota = group.Key == "Four-lane TX differential" ? 9 : 10;
                SparkTerminal previous = board.input;
                foreach (var row in group)
                {
                    int i = routes.Count;
                    string id = row[0];
                    int leg = i is >= 48 and < 56 ? (i - 48) * 2 : i is >= 88 and < 96 ? (i - 88) * 2 + 2 : -1;
                    // Alternate the two polarities within each lane and preserve independent TX/RX legs.
                    int lane = leg < 0 ? -1 : (i < 88 ? i - 48 : i - 88) / 2;
                    int conductor = leg < 0 ? -1 : (i < 88 ? 0 : 2) + i % 2;
                    bool clock = i is 24 or 25;
                    bool isReturn = id == ReturnTask;
                    bool functional = clock || lane >= 0 || isReturn;
                    string net = clock ? i == 24 ? "REFCLK_P" : "REFCLK_N" : isReturn ? "LANE2_RETURN" :
                        lane >= 0 ? $"LANE{lane}_{(conductor < 2 ? "TX" : "RX")}_{(conductor % 2 == 0 ? "P" : "N")}" : "CHECK_" + witnesses.Count;
                    string fromName = clock ? "U5" : isReturn ? "U8.GROUND" : conductor < 2 ? "U2" : "U8";
                    string toName = clock ? "U4" : isReturn ? "U8.RETURN_V37" : conductor < 2 ? "U8" : "U3";
                    Vector3 start = functional ? Site(fromName.Split('.')[0]) : sites[i % sites.Length];
                    Vector3 end = functional ? Site(toName.Split('.')[0]) : start;
                    if (isReturn) end += new Vector3(.018f, 0, .019f);
                    var entry = FindPad(start, i % 8, pads);
                    var destination = FindPad(end, (i + 2) % 8, pads);
                    var a = graph.Pin(functional ? fromName + "." + net + ".A" : id + ".A", entry, true);
                    var b = graph.Pin(functional ? toName + "." + net + ".B" : id + ".B", destination, true);
                    var route = new SparkLevel111Process.Route
                    {
                        id = id, group = group.Key, net = net, entry = entry, destination = destination,
                        layer = clock ? 5 : isReturn ? 3 : lane >= 0 ? conductor < 2 ? 3 : 7 : i % SparkLevel111Process.LayerCount + 1,
                        from = a, to = b, copper = Copper(graph.Root, mats[functional ? "Gold_ENIG" : "Copper_Exposed"], id, entry, destination)
                    };
                    var assemblyOut = functional ? graph.Pin(id + ".ASSEMBLY_CHECK") : a;
                    var solder = graph.SwitchedOutput(id + " solder witness", previous, assemblyOut, i >= board.parts.Length);
                    if (i < board.parts.Length) board.parts[i].contact = solder;
                    previous = functional ? assemblyOut : b;
                    if (clock)
                    {
                        var driver = graph.SwitchedOutput(net + " conditioner driver", board.aux, a);
                        var receiver = graph.Pin(i == 24 ? "U4.REFCLK_P_SENSE" : "U4.REFCLK_N_SENSE");
                        var series = graph.Load(net + " distribution", b, receiver, 68f);
                        receiver.transform.SetParent(series.transform, true);
                        graph.Load(net + " termination", receiver, board.ground, 10000f);
                        var noise = Shunt(net + " degraded PLL control load", receiver, 155f);
                        if (i == 24) { board.clockDriveP = driver; board.clockP = receiver; board.clockNoiseP = noise; }
                        else { board.clockDriveN = driver; board.clockN = receiver; board.clockNoiseN = noise; }
                    }
                    else if (isReturn)
                    {
                        board.returnRoute = i;
                        route.viaId = ReturnVia;
                        route.exitLayer = 4;
                        route.barrel = graph.SwitchedOutput("V37 lane 2 return microvia", b, board.returnSense);
                        graph.Load("Lane 2 reference plane", a, board.ground, 1f);
                        graph.Load("Lane 2 return continuity bias", board.aux, board.returnSense, 2200f);
                        board.returnDistanceMm = LengthMm(entry, destination);
                    }
                    else if (lane >= 0)
                    {
                        var driver = graph.SwitchedOutput(net + " symbol driver", board.retimerRail, a);
                        var receiver = graph.Pin(net + ".RECEIVER");
                        var channel = graph.Load(net + " channel and coupling witness", b, receiver, 18f);
                        receiver.transform.SetParent(channel.transform, true);
                        graph.Load(net + " receiver termination witness", receiver, board.ground, 330f);
                        board.lanes[lane].drive[conductor] = driver;
                        board.lanes[lane].sense[conductor] = receiver;
                        board.lanes[lane].lengthMm[conductor] = LengthMm(entry, destination);
                    }
                    else if (quota > 0)
                    {
                        quota--;
                        route.viaId = remaining.Dequeue();
                        route.layer = i % 13 + 1;
                        route.exitLayer = route.layer + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", destination);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", b, previous);
                    }
                    routes.Add(route);
                    csv.Add(Row(route));
                }
                graph.Load(group.Key + " continuity witness", previous, board.ground, 1000f);
                witnesses.Add(previous);
            }
            if (routes.Count != SparkLevel111Process.RouteCount || remaining.Count != 0 ||
                routes.Count(route => route.barrel) != SparkLevel111Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel111Process.LayerCount ||
                board.parts.Any(part => !part.contact) || board.lanes.Any(lane => lane.sense.Any(pin => !pin)))
                throw new InvalidOperationException("Incomplete SIX-19 training net, layer or via mapping.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();

            var senderPoint = Site("U18") + new Vector3(-.008f, .003f, .009f);
            var receiverPoint = Site("U19") + new Vector3(.008f, .003f, .009f);
            var sender = graph.Pin("U18.TRUSTED_LINK_HEALTH_OUT", senderPoint, true);
            var receiverPin = graph.Pin("U19.TRUSTED_LINK_HEALTH_IN", receiverPoint, true);
            board.heartbeat = graph.SwitchedOutput("U18 verified signal heartbeat", board.aux, sender);
            var monitorBias = graph.Load("U19 input bias", receiverPin, board.ground, 10000f);
            receiverPin.transform.SetParent(monitorBias.transform, true);
            board.jumper = new SparkLevel111Process.Route
            {
                id = "W185", group = "Independent safety", net = "TRUSTED_LINK_HEALTH", layer = 1,
                from = sender, to = receiverPin, entry = senderPoint, destination = receiverPoint,
                copper = Copper(graph.Root, mats["Gold_ENIG"], "W185", senderPoint, receiverPoint)
            };
            csv.Add(Row(board.jumper));
            board.monitorSupply = graph.Pin("U19.INDEPENDENT_3V3");
            var feed = graph.Load("U19 independent supply", board.aux, board.monitorSupply, 1f);
            board.monitorSupply.transform.SetParent(feed.transform, true);
            graph.Load("U19 monitor reference", board.monitorSupply, board.ground, 1000f);
            board.protectedFeed = graph.Pin("U19.PROTECTED_DIAGNOSTIC_WORKLOAD");
            board.workload = graph.SwitchedOutput("U19 independent workload inhibit", board.monitorSupply, board.protectedFeed);
            graph.Load("Protected workload witness", board.protectedFeed, board.ground, 330f);

            foreach (var point in new[] { senderPoint, receiverPoint })
                Primitive(graph.Root, "W185 contact", PrimitiveType.Cylinder, point, new Vector3(.003f, .00025f, .003f), mats["Gold_ENIG"]);
            board.locationMarker = Marker(graph.Root, "Placement locator", mats["Warning_Amber"], .0048f);
            board.startMarker = Marker(graph.Root, "Route source", mats["Gold_ENIG"], .003f);
            board.endMarker = Marker(graph.Root, "Route destination", mats["Signal_Cyan"], .003f);
            board.statusLamp = Primitive(graph.Root, "U19 independent permit lamp", PrimitiveType.Cube,
                receiverPoint + new Vector3(.007f, .001f, 0), new Vector3(.0026f, .0007f, .0018f), mats["Signal_Cyan"]);
            var locators = new[] { routes[board.returnRoute].destination, Site("U5"), Site("U8") };
            board.faultOverlays = locators.Select((point, index) =>
            {
                var marker = Marker(graph.Root, "Diagnosed fault " + (index + 1), mats["Fault_Red"], .0053f);
                marker.transform.localPosition = point + Vector3.up * .005f;
                marker.enabled = true;
                marker.gameObject.SetActive(false);
                return marker.gameObject;
            }).ToArray();
            var silk = new Color(.70f, .87f, .91f);
            BoardText(graph.Root, "SIX-19  /  THE SIGNAL CRUCIBLE", new Vector3(-.100f, .004f, -.105f), .00105f, Color.white);
            BoardText(graph.Root, "W185  /  U18 → U19", (senderPoint + receiverPoint) * .5f + Vector3.forward * .006f, .00064f, silk);
            BoardText(graph.Root, "U5  REFCLK", Site("U5") + new Vector3(0, .003f, -.013f), .00067f, silk);
            BoardText(graph.Root, "U8  RETIMER", Site("U8") + new Vector3(0, .003f, .012f), .00067f, silk);
            WorkLight(graph.Root, "Bench key", new Vector3(-.073f, .105f, -.055f), new Color(1f, .92f, .83f), 10.5f);
            WorkLight(graph.Root, "Channel fill", new Vector3(.073f, .105f, .053f), new Color(.71f, .87f, 1f), 10.2f);
            return csv;

            (SparkTerminal, SparkSwitch) Regulate(string name, float nominal)
            {
                var raw = graph.Pin(name + ".SOURCE");
                graph.Supply(name + " training regulator", raw, board.ground, nominal);
                var terminal = graph.Pin(name);
                var gate = graph.SwitchedOutput(name + " isolation", raw, terminal);
                graph.Load(name + " witness", terminal, board.ground, 1000f);
                return (terminal, gate);
            }

            SparkSwitch Shunt(string name, SparkTerminal input, float resistance)
            {
                var branch = graph.Pin(name + ".OUT");
                var gate = graph.SwitchedOutput(name, input, branch);
                graph.Load(name + " load", branch, board.ground, resistance);
                return gate;
            }
        }

        private static float LengthMm(Vector3 a, Vector3 b) => (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z)) * 1000f;

        private static string Row(SparkLevel111Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 site, int offset, List<Vector3> used)
        {
            for (int ring = 0; ring < 10; ring++)
            for (int step = 0; step < 8; step++)
            {
                float angle = (offset + step) * Mathf.PI / 4f;
                float radius = .0048f + .0023f * ring;
                var point = new Vector3(site.x + radius * Mathf.Cos(angle), .0046f, site.z + radius * Mathf.Sin(angle));
                if (Mathf.Abs(point.x) > .161f || Mathf.Abs(point.z) > .115f ||
                    used.Exists(other => (other - point).sqrMagnitude < .0000027f)) continue;
                used.Add(point);
                return point;
            }
            throw new InvalidOperationException("No free SIX-19 terminal near " + site);
        }
    }
}
