using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.AutonomySentinel;
using UnityEngine;

namespace ProjectSpark.EditorTools.AutonomySentinel
{
    internal static partial class SparkLevel122Builder
    {
        private static List<string> ConfigureGraph(SparkLevel122Process board, Circuit graph, string[][] plans,
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

            var routes = new List<SparkLevel122Process.Route>(SparkLevel122Process.RouteCount);
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
                int quota = witnesses.Count < 4 ? 19 : 18;
                int within = 0;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = (group.Key, within) switch
                    {
                        ("ProtectedPower", 0) => "U24.PROTECTED_POWER",
                        ("GNSS_RF", 0) => "U6.GNSS_A_RF",
                        ("GNSS_RF", 1) => "U7.GNSS_B_RF",
                        ("Radar_RF", 0) => "U8.RADAR_A_RETURN",
                        ("Radar_RF", 1) => "U9.RADAR_B_RETURN",
                        ("Radar_RF", 2) => "U18.RADAR_PLL_TRIM",
                        ("InertialTiming", 0) => "U10.IMU_A_RETURN",
                        ("InertialTiming", 1) => "U11.IMU_B_RETURN",
                        ("InertialTiming", 2) => "U17.TIMEBASE",
                        _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 59) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel122Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + group.Key,
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel122Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "copper" : "gold"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel122Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.00225f, .0002f, .00225f), materials["via"]);
                        route.viaVisual.enabled = false;
                        if (role != null) sensorReturns.Add(role, previous);
                        if (role == "U6.GNSS_A_RF") board.gnssViaIndex = index;
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                    within++;
                }
                graph.Load(group.Key + " continuity return", previous, board.ground, 1800f);
                witnesses.Add(group.Key, previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel122Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel122Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel122Process.LayerCount ||
                board.parts.Any(part => !part.contact) || !sensorReturns.ContainsKey("U6.GNSS_A_RF"))
                throw new InvalidOperationException("The ANX30-EDU-1 topology is incomplete.");

            board.routes = routes.ToArray();
            board.witnesses = witnesses.Values.ToArray();
            (board.rail, board.bench) = Sense("U24.PROTECTED_BENCH_5V", sensorReturns["U24.PROTECTED_POWER"]);
            (board.monitorSupply, board.monitorGate) = Sense("U4.INDEPENDENT_DIAGNOSTIC_5V", board.input);
            (board.clockPin, board.clockGate) = Sense("U17.TIMEBASE_LIVE", sensorReturns["U17.TIMEBASE"]);
            var pllTest = Sense("U18.PLL_TEST_SUPPLY", sensorReturns["U18.RADAR_PLL_TRIM"]);
            board.pllGate = pllTest.edge;
            (board.pllPin, board.pllContact) = Sense("U18.PLL_CONDITIONED_CLOCK", pllTest.pin, true);
            var imuTest = Sense("U11.REFERENCE_TEST_SUPPLY", board.rail);
            board.referenceGate = imuTest.edge;
            (board.referencePin, board.imuContact) = Sense("U11.IMU_B_REFERENCE", imuTest.pin, true);
            board.navWitness = witnesses["SensorFusion"];
            board.sensorPins = new SparkTerminal[6];
            board.sensorGates = new SparkSwitch[6];
            board.sensorMarkers = new Renderer[6];
            string[] roles = { "U6.GNSS_A_RF", "U7.GNSS_B_RF", "U8.RADAR_A_RETURN", "U9.RADAR_B_RETURN",
                "U10.IMU_A_RETURN", "U11.IMU_B_RETURN" };
            string[] labels = { "GNSS A", "GNSS B", "RADAR A", "RADAR B", "IMU A", "IMU B" };
            for (int i = 0; i < roles.Length; i++)
            {
                (board.sensorPins[i], board.sensorGates[i]) = Sense(roles[i] + ".LIVE", sensorReturns[roles[i]]);
                if (i == 0)
                    (board.sensorPins[0], board.rfContact) = Sense("U6.GNSS_A_CONTINUITY", board.sensorPins[0], true);
                Vector3 site = Site(roles[i].Split('.')[0]);
                var marker = Marker(graph.Root, labels[i] + " status", materials["blue"], .0042f);
                marker.transform.localPosition = site + new Vector3(0, .024f, -.022f);
                board.sensorMarkers[i] = marker;
                BoardText(graph.Root, labels[i], site + new Vector3(0, .002f, -.030f), .00093f, Color.white);
            }
            Vector3 a = FindPad(Site("U5"), 6, pads);
            Vector3 b = FindPad(Site("U4"), 2, pads);
            var sender = graph.Pin("U5.TRUSTED_LOCALIZATION_READY_OUT", a, true);
            var receiver = graph.Pin("U4.TRUSTED_LOCALIZATION_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U5 localization-ready heartbeat", board.monitorSupply, sender);
            var pullDown = graph.Load("U4 independent feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel122Process.Route
            {
                id = "W385", group = "Independent supervision", net = "TRUSTED_LOCALIZATION_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["gold"], "W385", a, b)
            };
            map.Add(Row(board.jumper));
            (board.protectedFeed, board.workload) = Sense("U4.PROTECTED_MOTION_PERMIT", board.monitorSupply);
            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W385 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["gold"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["gold"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["blue"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["blue"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U4 motion permit indicator", PrimitiveType.Cube,
                b + new Vector3(.006f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["green"]);
            board.faultOverlays = new[] { Site("U18"), board.routes[board.gnssViaIndex].destination, Site("U11") }
                .Select((point, i) =>
                {
                    var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["red"], .012f);
                    marker.transform.localPosition = point + Vector3.up * .013f;
                    marker.enabled = true;
                    marker.gameObject.SetActive(false);
                    return marker.gameObject;
                }).ToArray();
            BoardText(graph.Root, "ANX-30  /  THE AUTONOMY SENTINEL", new Vector3(0, .0017f, -.196f), .00132f, Color.white);
            BoardText(graph.Root, "W385  /  U4", (a + b) * .5f + Vector3.right * .022f, .00091f, Color.white);
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

        private static string Row(SparkLevel122Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000f:F3},{route.entry.z * 1000f:F3},{route.destination.x * 1000f:F3},{route.destination.z * 1000f:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int start, List<Vector3> occupied)
        {
            for (int offset = 0; offset < 128; offset++)
            {
                int step = (start + offset) % 8;
                float angle = step * Mathf.PI * .25f;
                float radius = .0068f + (offset / 8) * .0025f;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .273f || Mathf.Abs(pad.z) > .203f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free ANX30-EDU-1 contact at " + near);
        }
    }
}
