using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.PerceptionNexus;
using UnityEngine;

namespace ProjectSpark.EditorTools.PerceptionNexus
{
    internal static partial class SparkLevel121Builder
    {
        private static List<string> ConfigureGraph(SparkLevel121Process board, Circuit graph, string[][] plans,
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

            var routes = new List<SparkLevel121Process.Route>(SparkLevel121Process.RouteCount);
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
                int quota = witnesses.Count < 8 ? 13 : 12;
                int within = 0;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = within == 0 ? group.Key switch
                    {
                        "PowerProtection" => "J9.INPUT_RETURN",
                        "Camera01" => "J1.CAM_A_RETURN",
                        "Camera02" => "J2.CAM_B_VIA",
                        "Camera03" => "J3.CAM_C_RETURN",
                        "Camera04" => "J4.CAM_D_RETURN",
                        "PPS_Clock" => "U4.PPS_REFERENCE",
                        "LiDAR_ToF" => "C12.LIDAR_BIAS_WITNESS",
                        "IMU_Trigger" => "J6.IMU_RETURN",
                        _ => null
                    } : null;
                    Vector3 site = role == null ? sites[(index * 37) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel121Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + group.Key,
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel121Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "copper" : "gold"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel121Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.00225f, .0002f, .00225f), materials["solder"]);
                        route.viaVisual.enabled = false;
                        if (role != null) sensorReturns.Add(role, previous);
                        if (role == "J2.CAM_B_VIA") board.cameraViaIndex = index;
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                    within++;
                }
                graph.Load(group.Key + " continuity return", previous, board.ground, 1800f);
                witnesses.Add(group.Key, previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel121Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel121Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel121Process.LayerCount ||
                board.parts.Any(part => !part.contact) || !sensorReturns.ContainsKey("J2.CAM_B_VIA"))
                throw new InvalidOperationException("The PNX29-EDU-1 topology is incomplete.");

            board.routes = routes.ToArray();
            board.witnesses = witnesses.Values.ToArray();
            (board.rail, board.bench) = Sense("PNX29.PROTECTED_BENCH_5V", board.input);
            (board.monitorSupply, board.monitorGate) = Sense("U6.INDEPENDENT_DIAGNOSTIC_5V", board.input);
            (board.clockPin, board.clockGate) = Sense("U4.PPS_REFERENCE_LIVE", sensorReturns["U4.PPS_REFERENCE"]);
            (board.trimPin, board.trimGate) = Sense("U4.TRIGGER_DISCIPLINE", board.clockPin);
            (board.resetPin, board.resetGate) = Sense("U2.FPGA_RESET_RELEASE", board.rail);
            (board.imuPin, board.imuGate) = Sense("J6.IMU_TRIGGER", sensorReturns["J6.IMU_RETURN"]);
            (board.lidarBias, board.lidarGate) = Sense("J5.RANGING_ENABLE", sensorReturns["C12.LIDAR_BIAS_WITNESS"]);
            board.fusionWitness = witnesses["FPGA_Fusion"];
            board.lidarPin = graph.Pin("C12.FILTERED_LIDAR_5V");
            // In this fictional, averaged filter model the load-step loss is solved by
            // the native circuit through a separate series-loss resistor.
            board.filterLoss = graph.Load("C12 filter series loss", board.rail, board.lidarPin, .12f);
            board.lidarPin.transform.SetParent(board.filterLoss.transform, true);
            graph.Load("LiDAR idle load", board.lidarPin, board.ground, 750f);
            var sample = graph.Pin("J5.ECHO_LOAD");
            board.lidarPulse = graph.SwitchedOutput("LiDAR echo load step", board.lidarPin, sample);
            graph.Load("LiDAR pulse load", sample, board.ground, 72f);
            board.cameraPins = new SparkTerminal[4];
            board.cameraGates = new SparkSwitch[4];
            board.sensorMarkers = new Renderer[6];
            string[] roles = { "J1.CAM_A_RETURN", "J2.CAM_B_VIA", "J3.CAM_C_RETURN", "J4.CAM_D_RETURN" };
            for (int i = 0; i < board.sensorMarkers.Length; i++)
            {
                if (i < 4)
                    (board.cameraPins[i], board.cameraGates[i]) = Sense($"J{i + 1}.CAM_{(char)('A' + i)}_VALID", sensorReturns[roles[i]]);
                string label = i < 4 ? $"CAM {(char)('A' + i)}" : i == 4 ? "LIDAR" : "IMU";
                Vector3 site = Site("J" + (i + 1));
                var marker = Marker(graph.Root, label + " status", materials["led_cyan"], .0057f);
                marker.transform.localPosition = new Vector3(.208f, .009f, site.z);
                board.sensorMarkers[i] = marker;
                BoardText(graph.Root, label, new Vector3(.187f, .0017f, site.z - .013f), .0010f, Color.white);
            }
            Vector3 a = FindPad(Site("U5"), 6, pads);
            Vector3 b = FindPad(Site("U6"), 2, pads);
            var sender = graph.Pin("U5.TRUSTED_PERCEPTION_READY_OUT", a, true);
            var receiver = graph.Pin("U6.TRUSTED_PERCEPTION_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U5 perception-ready heartbeat", board.monitorSupply, sender);
            var pullDown = graph.Load("U6 independent feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel121Process.Route
            {
                id = "W369", group = "Independent supervision", net = "TRUSTED_PERCEPTION_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["gold"], "W369", a, b)
            };
            map.Add(Row(board.jumper));
            (board.protectedFeed, board.workload) = Sense("U6.PROTECTED_PERCEPTION_PERMIT", board.monitorSupply);
            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W369 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["gold"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["gold"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["led_cyan"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["led_cyan"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U6 perception permit indicator", PrimitiveType.Cube,
                b + new Vector3(.006f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["led_green"]);
            board.faultOverlays = new[] { board.routes[board.cameraViaIndex].destination, Site("U4"), Site("C12") }
                .Select((point, i) =>
                {
                    var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["led_red"], .012f);
                    marker.transform.localPosition = point + Vector3.up * .013f;
                    marker.enabled = true;
                    marker.gameObject.SetActive(false);
                    return marker.gameObject;
                }).ToArray();
            BoardText(graph.Root, "PNX-29  /  THE PERCEPTION NEXUS", new Vector3(0, .0017f, -.186f), .00132f, Color.white);
            BoardText(graph.Root, "W369  /  U6", (a + b) * .5f + Vector3.right * .022f, .00091f, Color.white);
            WorkLight(graph.Root, "Neutral task light", new Vector3(-.125f, .149f, -.062f), new Color(1f, .94f, .86f), 4.9f);
            WorkLight(graph.Root, "Cool task fill", new Vector3(.118f, .138f, .044f), new Color(.78f, .90f, 1f), 4.1f);
            return map;

            (SparkTerminal pin, SparkSwitch edge) Sense(string label, SparkTerminal upstream)
            {
                var pin = graph.Pin(label);
                var edge = graph.SwitchedOutput(label + " contact", upstream, pin);
                graph.Load(label + " monitor return", pin, board.ground, 12000f);
                return (pin, edge);
            }
        }

        private static string Row(SparkLevel121Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000f:F3},{route.entry.z * 1000f:F3},{route.destination.x * 1000f:F3},{route.destination.z * 1000f:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int start, List<Vector3> occupied)
        {
            for (int offset = 0; offset < 128; offset++)
            {
                int step = (start + offset) % 8;
                float angle = step * Mathf.PI * .25f;
                float radius = .0068f + (offset / 8) * .0025f;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .263f || Mathf.Abs(pad.z) > .193f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free PNX29-EDU-1 contact at " + near);
        }
    }
}
