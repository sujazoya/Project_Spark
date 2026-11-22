using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.FlightSovereign;
using UnityEngine;

namespace ProjectSpark.EditorTools.FlightSovereign
{
    internal static partial class SparkLevel123Builder
    {
        private static List<string> ConfigureGraph(SparkLevel123Process board, Circuit graph, string[][] plans,
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

            var routes = new List<SparkLevel123Process.Route>(SparkLevel123Process.RouteCount);
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
                int quota = 26;
                int within = 0;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = (group.Key, within) switch
                    {
                        ("PowerGround", 0) => "U6.CONTROLLER_POWER",
                        ("PowerGround", 1) => "U5.BENCH_LOAD_POWER",
                        ("FlightClockReset", 0) => "Y1.CLOCK_A",
                        ("FlightClockReset", 1) => "Y2.CLOCK_B",
                        ("FlightClockReset", 2) => "U6.RESET_RELEASE",
                        ("RedundantBus", 0) => "U1.FCU_A",
                        ("RedundantBus", 1) => "U2.FCU_B",
                        ("MotorControl", < 8) => $"J{within + 1}.STAGE_RETURN",
                        ("SensorSync", 0) => "U7.IMU_A",
                        ("SensorSync", 1) => "U8.IMU_B_SPI",
                        ("SensorSync", 2) => "U9.IMU_C",
                        ("SensorSync", 3) => "U10.BARO",
                        ("SensorSync", 4) => "U11.GNSS_A",
                        ("SensorSync", 5) => "U12.GNSS_B",
                        _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 59) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel123Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + group.Key,
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel123Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "copper" : "gold"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel123Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.00225f, .0002f, .00225f), materials["solder"]);
                        route.viaVisual.enabled = false;
                        if (role != null) sensorReturns.Add(role, previous);
                        if (role == "U8.IMU_B_SPI") board.imuViaIndex = index;
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                    within++;
                }
                graph.Load(group.Key + " continuity return", previous, board.ground, 1800f);
                witnesses.Add(group.Key, previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel123Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel123Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel123Process.LayerCount ||
                board.parts.Any(part => !part.contact) || !sensorReturns.ContainsKey("U8.IMU_B_SPI"))
                throw new InvalidOperationException("The AFX31-EDU-1 topology is incomplete.");

            board.routes = routes.ToArray();
            board.witnesses = witnesses.Values.ToArray();
            (board.rail, board.bench) = Sense("U6.CONTROLLER_RAIL", sensorReturns["U6.CONTROLLER_POWER"]);
            (board.driveRail, board.drivePower) = Sense("U5.BENCH_LOAD_RAIL", sensorReturns["U5.BENCH_LOAD_POWER"]);
            (board.monitorSupply, board.monitorGate) = Sense("U3.INDEPENDENT_DIAGNOSTIC_5V", board.input);
            (board.clockPin, board.clockGate) = Sense("Y1.TIMEBASE_A_LIVE", sensorReturns["Y1.CLOCK_A"]);
            (board.secondaryClock, board.secondaryClockGate) = Sense("Y2.TIMEBASE_B_LIVE", sensorReturns["Y2.CLOCK_B"]);
            (board.resetPin, board.resetGate) = Sense("U6.RESET_RELEASE_LIVE", sensorReturns["U6.RESET_RELEASE"]);
            board.logicPins = new SparkTerminal[2];
            board.logicGates = new SparkSwitch[2];
            (board.logicPins[0], board.logicGates[0]) = Sense("U1.CONTROL_A_LIVE", sensorReturns["U1.FCU_A"]);
            (board.logicPins[1], board.logicGates[1]) = Sense("U2.CONTROL_B_LIVE", sensorReturns["U2.FCU_B"]);
            board.motorPins = new SparkTerminal[8];
            board.motorPower = new SparkSwitch[8];
            board.gatePins = new SparkTerminal[8];
            board.outputContacts = new SparkSwitch[8];
            board.motorLoads = new SparkResistor[8];
            board.sensorMarkers = new Renderer[8];
            for (int i = 0; i < board.motorPins.Length; i++)
            {
                string siteId = "J" + (i + 1);
                string trace = siteId + ".STAGE_RETURN";
                var monitor = Sense(trace + ".BENCH", sensorReturns[trace]);
                board.motorPower[i] = monitor.edge;
                board.motorPins[i] = monitor.pin;
                if (i == 4)
                    (board.motorPins[i], board.motorContact) = Sense(siteId + ".HIGH_SIDE_GATE_TEST", monitor.pin, true);
                board.motorLoads[i] = graph.Load(siteId + " training current shunt", board.motorPins[i], board.ground, 100f);

                Vector3 site = Site(siteId);
                var marker = Marker(graph.Root, "Channel " + (i + 1) + " status", materials["blue"], .0040f);
                marker.transform.localPosition = site + new Vector3(-.021f, .011f, 0);
                board.sensorMarkers[i] = marker;
                BoardText(graph.Root, "M" + (i + 1), site + new Vector3(-.027f, .002f, -.012f), .0011f, Color.white);
            }

            board.sensorPins = new SparkTerminal[6];
            board.sensorGates = new SparkSwitch[6];
            string[] roles = { "U7.IMU_A", "U8.IMU_B_SPI", "U9.IMU_C", "U10.BARO", "U11.GNSS_A", "U12.GNSS_B" };
            string[] labels = { "IMU A", "IMU B", "IMU C", "BARO", "GNSS A", "GNSS B" };
            for (int i = 0; i < roles.Length; i++)
            {
                var monitor = Sense(roles[i] + ".LIVE", sensorReturns[roles[i]]);
                board.sensorGates[i] = monitor.edge;
                board.sensorPins[i] = monitor.pin;
                if (i == 1)
                    (board.sensorPins[1], board.imuContact) = Sense("U8.SPI_VIA_CONTACT", monitor.pin, true);

                Vector3 original = Site(roles[i].Split('.')[0]);
                BoardText(graph.Root, labels[i], original + new Vector3(0, .002f, -.025f), .001f, Color.white);
            }
            board.barometerReference = graph.Pin("U10.BARO_REFERENCE");
            board.baroContact = graph.SwitchedOutput("U10 barometer reference trim", board.sensorPins[3], board.barometerReference, true);
            graph.Load("Barometer reference bias", board.sensorPins[3], board.barometerReference, 330f);
            graph.Load("Barometer reference return", board.barometerReference, board.ground, 1000f);

            Vector3 a = FindPad(Site("U5"), 6, pads);
            Vector3 b = FindPad(Site("U3"), 2, pads);
            var sender = graph.Pin("U5.TRUSTED_FLIGHT_READY_OUT", a, true);
            var receiver = graph.Pin("U3.TRUSTED_FLIGHT_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U5 flight-ready heartbeat", board.monitorSupply, sender);
            var pullDown = graph.Load("U3 independent feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel123Process.Route
            {
                id = "W401", group = "Independent supervision", net = "TRUSTED_FLIGHT_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["gold"], "W401", a, b)
            };
            map.Add(Row(board.jumper));
            (board.protectedFeed, board.workload) = Sense("U3.PROTECTED_STAND_PERMIT", board.driveRail);
            for (int i = 0; i < board.gatePins.Length; i++)
                (board.gatePins[i], board.outputContacts[i]) = Sense($"J{i + 1}.PROTECTED_GATE", board.protectedFeed);
            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W401 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["gold"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["gold"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["blue"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["blue"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U3 stand permit indicator", PrimitiveType.Cube,
                b + new Vector3(.006f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["green"]);
            board.faultOverlays = new[] { Site("J5"), board.routes[board.imuViaIndex].destination, Site("U10") }
                .Select((point, i) =>
                {
                    var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["red"], .012f);
                    marker.transform.localPosition = point + Vector3.up * .013f;
                    marker.enabled = true;
                    marker.gameObject.SetActive(false);
                    return marker.gameObject;
                }).ToArray();
            BoardText(graph.Root, "AFX-31  /  THE FLIGHT SOVEREIGN", new Vector3(0, .0017f, -.211f), .00132f, Color.white);
            BoardText(graph.Root, "W401  /  U3", (a + b) * .5f + Vector3.right * .022f, .00091f, Color.white);
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

        private static string Row(SparkLevel123Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000f:F3},{route.entry.z * 1000f:F3},{route.destination.x * 1000f:F3},{route.destination.z * 1000f:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int start, List<Vector3> occupied)
        {
            for (int offset = 0; offset < 128; offset++)
            {
                int step = (start + offset) % 8;
                float angle = step * Mathf.PI * .25f;
                float radius = .0068f + (offset / 8) * .0025f;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .283f || Mathf.Abs(pad.z) > .213f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free AFX31-EDU-1 contact at " + near);
        }
    }
}
