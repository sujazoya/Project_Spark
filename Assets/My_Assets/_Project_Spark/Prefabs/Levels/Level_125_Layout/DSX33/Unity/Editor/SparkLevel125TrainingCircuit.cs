using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.SignalHorizon;
using UnityEngine;

namespace ProjectSpark.EditorTools.SignalHorizon
{
    internal static partial class SparkLevel125Builder
    {
        private static List<string> ConfigureGraph(SparkLevel125Process board, Circuit graph, string[][] plans,
            IEnumerable<string> viaIds, Dictionary<string, Material> materials)
        {
            Vector3 Site(string reference) =>
                graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == reference).footprint.position);

            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 5f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity test return", board.input, board.ground, 10000f);

            var routes = new List<SparkLevel125Process.Route>(SparkLevel125Process.RouteCount);
            var witnesses = new Dictionary<string, SparkTerminal>(StringComparer.Ordinal);
            var availableVias = new Queue<string>(viaIds);
            var pads = new List<Vector3>();
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var map = new List<string>
            {
                "task,domain,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer"
            };
            var sensorReturns = new Dictionary<string, SparkTerminal>();
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                var previous = board.input;
                int quota = witnesses.Count < 4 ? 22 : 21;
                int within = 0;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = (group.Key, within) switch
                    {
                        ("ProtectedPowerReturn", 0) => "J7.CONTROLLER_POWER",
                        ("ProtectedPowerReturn", 1) => "M7.TRANSMITTER_LOAD_POWER",
                        ("ReferenceClock", 0) => "Y1.CLOCK_A",
                        ("ReferenceClock", 1) => "Y2.CLOCK_B",
                        ("ReferenceClock", 2) => "Y1.REFERENCE_TRIM",
                        ("IndependentInhibit", 0) => "U3.RESET_RELEASE",
                        ("ReceiverA", 0) => "M1.RX_A",
                        ("ReceiverB", 0) => "M2.RX_B",
                        ("ReceiverB", 1) => "C1.RECEIVE_FILTER",
                        ("TransmitterA", 0) => "Q1.TX_A_RETURN",
                        ("TransmitterB", 0) => "M4.TX_B_RETURN",
                        ("FEC_Telemetry", 0) => "U1.COMPUTE_A",
                        ("FEC_Telemetry", 1) => "U2.COMPUTE_B",
                        ("FEC_Telemetry", 2) => "U4.FEC",
                        ("FEC_Telemetry", 3) => "U5.DIAGNOSTICS",
                        _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 59) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel125Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + group.Key,
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel125Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "copper" : "gold"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel125Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.00225f, .0002f, .00225f), materials["solder"]);
                        route.viaVisual.enabled = false;
                        if (role != null) sensorReturns.Add(role, previous);
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                    within++;
                }
                graph.Load(group.Key + " continuity return", previous, board.ground, 1800f);
                witnesses.Add(group.Key, previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel125Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel125Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel125Process.LayerCount ||
                board.parts.Any(part => !part.contact) || !sensorReturns.ContainsKey("C1.RECEIVE_FILTER"))
                throw new InvalidOperationException("The DSX33-EDU-1 topology is incomplete.");

            board.routes = routes.ToArray();
            board.witnesses = witnesses.Values.ToArray();
            (board.rail, board.bench) = Sense("J7.CONTROLLER_RAIL", sensorReturns["J7.CONTROLLER_POWER"]);
            (board.driveRail, board.drivePower) = Sense("M7.TRANSMITTER_LOAD_RAIL", sensorReturns["M7.TRANSMITTER_LOAD_POWER"]);
            (board.monitorSupply, board.monitorGate) = Sense("U3.INDEPENDENT_DIAGNOSTIC_5V", board.input);
            (board.telemetryPin, board.telemetryGate) = Sense("U5.DIAGNOSTIC_TELEMETRY", sensorReturns["U5.DIAGNOSTICS"]);
            (board.clockPin, board.clockGate) = Sense("Y1.TIMEBASE_A_LIVE", sensorReturns["Y1.CLOCK_A"]);
            (board.secondaryClock, board.secondaryClockGate) = Sense("Y2.TIMEBASE_B_LIVE", sensorReturns["Y2.CLOCK_B"]);
            var clockFixture = Sense("Y1.TRIM_TEST_RAIL", sensorReturns["Y1.REFERENCE_TRIM"]);
            board.trimPower = clockFixture.edge;
            (board.trimPin, board.clockContact) = Sense("Y1.TRIM_REFERENCE", clockFixture.pin, true);
            (board.resetPin, board.resetGate) = Sense("U3.RESET_RELEASE_LIVE", sensorReturns["U3.RESET_RELEASE"]);
            board.logicPins = new SparkTerminal[3];
            board.logicGates = new SparkSwitch[3];
            (board.logicPins[0], board.logicGates[0]) = Sense("U1.COMPUTE_A_LIVE", sensorReturns["U1.COMPUTE_A"]);
            (board.logicPins[1], board.logicGates[1]) = Sense("U2.COMPUTE_B_LIVE", sensorReturns["U2.COMPUTE_B"]);
            (board.logicPins[2], board.logicGates[2]) = Sense("U4.DECODER_LIVE", sensorReturns["U4.FEC"]);

            board.transmitterPins = new SparkTerminal[2];
            board.transmitterPower = new SparkSwitch[2];
            board.gatePins = new SparkTerminal[2];
            board.outputContacts = new SparkSwitch[2];
            board.transmitterLoads = new SparkResistor[2];
            string[] returns = { "Q1.TX_A_RETURN", "M4.TX_B_RETURN" };
            for (int i = 0; i < returns.Length; i++)
            {
                (board.transmitterPins[i], board.transmitterPower[i]) = Sense(returns[i] + ".BENCH", sensorReturns[returns[i]]);
                board.transmitterLoads[i] = graph.Load(returns[i] + " training current shunt", board.transmitterPins[i], board.ground, 100f);
            }

            board.sensorPins = new SparkTerminal[2];
            board.sensorGates = new SparkSwitch[2];
            (board.sensorPins[0], board.sensorGates[0]) = Sense("M1.RECEIVE_A_LIVE", sensorReturns["M1.RX_A"]);
            (board.sensorPins[1], board.sensorGates[1]) = Sense("M2.RECEIVE_B_LIVE", sensorReturns["M2.RX_B"]);
            var filterFixture = Sense("C1.FILTER_TEST_RAIL", sensorReturns["C1.RECEIVE_FILTER"]);
            board.filterPower = filterFixture.edge;
            board.filterPin = graph.Pin("C1.FILTER_OUTPUT");
            board.filterSeries = graph.Load("C1 equivalent series loss", filterFixture.pin, board.filterPin, .12f);
            board.filterPin.transform.SetParent(board.filterSeries.transform, true);
            graph.Load("RX B front-end test load", board.filterPin, board.ground, 61f);

            board.sensorMarkers = new Renderer[4];
            string[] labels = { "RX A", "RX B", "TX A", "TX B" };
            for (int i = 0; i < labels.Length; i++)
            {
                Vector3 site = Site("J" + (i + 1));
                var marker = Marker(graph.Root, labels[i] + " status", materials["blue"], .0042f);
                marker.transform.localPosition = site + new Vector3(-.021f, .016f, 0f);
                board.sensorMarkers[i] = marker;
                BoardText(graph.Root, labels[i], site + new Vector3(-.026f, .0028f, -.018f), .00115f, Color.white);
            }

            Vector3 a = FindPad(Site("U5"), 6, pads);
            Vector3 b = FindPad(Site("U3"), 2, pads);
            var sender = graph.Pin("U5.TRUSTED_TX_READY_OUT", a, true);
            var receiver = graph.Pin("U3.TRUSTED_TX_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U5 transmitter-ready heartbeat", board.monitorSupply, sender);
            var pullDown = graph.Load("U3 independent feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel125Process.Route
            {
                id = "W433", group = "Independent supervision", net = "TRUSTED_TX_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["gold"], "W433", a, b)
            };
            map.Add(Row(board.jumper));
            (board.protectedFeed, board.workload) = Sense("U3.PROTECTED_TX_PERMIT", board.driveRail);
            for (int i = 0; i < board.gatePins.Length; i++)
                (board.gatePins[i], board.outputContacts[i]) = Sense($"J{i + 3}.PROTECTED_TX_GATE", board.protectedFeed);
            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W433 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["gold"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["gold"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["blue"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["blue"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U3 transmitter permit indicator", PrimitiveType.Cube,
                b + new Vector3(.006f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["green"]);
            board.faultOverlays = new[] { Site("Y1"), Site("C1"), Site("Q1") }
                .Select((point, i) =>
                {
                    var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["red"], .012f);
                    marker.transform.localPosition = point + Vector3.up * .013f;
                    marker.enabled = true;
                    marker.gameObject.SetActive(false);
                    return marker.gameObject;
                }).ToArray();
            BoardText(graph.Root, "DSX-33  /  THE SIGNAL HORIZON", new Vector3(0, .0017f, -.230f), .00132f, Color.white);
            BoardText(graph.Root, "W433  /  U3", (a + b) * .5f + Vector3.right * .022f, .00091f, Color.white);
            WorkLight(graph.Root, "Neutral task light", new Vector3(-.125f, .149f, -.062f), new Color(1f, .94f, .86f), 4.9f);
            WorkLight(graph.Root, "Cool task fill", new Vector3(.118f, .138f, .044f), new Color(.78f, .90f, 1f), 4.1f);
            return map;

            (SparkTerminal pin, SparkSwitch edge) Sense(string label, SparkTerminal upstream, bool closed = false)
            {
                var pin = graph.Pin(label);
                var edge = graph.SwitchedOutput(label + " contact", upstream, pin, closed);
                graph.Load(label + " monitor return", pin, board.ground, 12000f);
                return (pin, edge);
            }
        }

        private static string Row(SparkLevel125Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000f:F3},{route.entry.z * 1000f:F3},{route.destination.x * 1000f:F3},{route.destination.z * 1000f:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int start, List<Vector3> occupied)
        {
            for (int offset = 0; offset < 128; offset++)
            {
                int step = (start + offset) % 8;
                float angle = step * Mathf.PI * .25f;
                float radius = .0068f + (offset / 8) * .0025f;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .303f || Mathf.Abs(pad.z) > .233f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free DSX33-EDU-1 contact at " + near);
        }
    }
}
