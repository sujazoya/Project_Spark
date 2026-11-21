using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.DeterministicGrid;
using UnityEngine;

namespace ProjectSpark.EditorTools.DeterministicGrid
{
    internal static partial class SparkLevel118Builder
    {
        private static List<string> ConfigureGraph(SparkLevel118Process board, Circuit graph, string[][] plans,
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

            var routes = new List<SparkLevel118Process.Route>(SparkLevel118Process.RouteCount);
            var witnesses = new Dictionary<string, SparkTerminal>(StringComparer.Ordinal);
            var availableVias = new Queue<string>(viaIds);
            var pads = new List<Vector3>();
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var map = new List<string>
            {
                "task,domain,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer"
            };
            var routeWitnesses = new Dictionary<string, SparkTerminal>(StringComparer.Ordinal);
            SparkTerminal pathBVia = null;
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                var previous = board.input;
                int quota = witnesses.Count < 6 ? 11 : 10;
                int within = 0;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = group.Key switch
                    {
                        "Clock_gPTP" when within == 0 => "Y1.PRIMARY_TIMEBASE",
                        "Clock_Holdover" when within == 0 => "Y2.HOLDOVER_TIMEBASE",
                        "Scheduler" when within == 0 => "U15.GATE_MEMORY_D0",
                        "Redundancy" when within == 0 => "U7.PATH_A_RETURN",
                        "Redundancy" when within == 1 => "U10.PATH_B_VIA",
                        "TSN_Ports_A" when within < 3 => $"J{within + 1}.PHY_RETURN",
                        "TSN_Ports_B" when within < 3 => $"J{within + 4}.PHY_RETURN",
                        _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 37) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel118Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + group.Key,
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel118Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "copper" : "gold"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel118Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.00225f, .0002f, .00225f), materials["solder"]);
                        route.viaVisual.enabled = false;
                        if (role != null) routeWitnesses.Add(role, previous);
                        if (role == "U10.PATH_B_VIA")
                        {
                            board.pathViaIndex = index;
                            pathBVia = previous;
                        }
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                    within++;
                }
                graph.Load(group.Key + " continuity return", previous, board.ground, 1800f);
                witnesses.Add(group.Key, previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel118Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel118Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel118Process.LayerCount ||
                board.parts.Any(part => !part.contact) || !pathBVia)
                throw new InvalidOperationException("The TSX26-EDU-1 topology is incomplete.");

            board.routes = routes.ToArray();
            board.witnesses = witnesses.Values.ToArray();
            (board.rail, board.bench) = Sense("TSX26.BENCH_5V", witnesses["Power_Return"]);
            (board.monitorSupply, board.monitorGate) = Sense("U32.INDEPENDENT_DIAGNOSTIC_5V", board.input);
            (board.primaryPin, board.primaryGate) = Sense("Y1.SUPPLY_VALID", routeWitnesses["Y1.PRIMARY_TIMEBASE"]);
            (board.trimPin, board.trimGate) = Sense("Y1.DISCIPLINE_LINK", board.primaryPin);
            (board.backupPin, board.backupGate) = Sense("Y2.SUPPLY_VALID", routeWitnesses["Y2.HOLDOVER_TIMEBASE"]);
            (board.pathA, board.pathGateA) = Sense("U7.PATH_A_CONTINUITY", routeWitnesses["U7.PATH_A_RETURN"]);
            (board.pathB, board.pathGateB) = Sense("U10.PATH_B_CONTINUITY", pathBVia);
            (board.sramPin, board.sramGate) = Sense("U15.SCHEDULE_D0_RETURN", routeWitnesses["U15.GATE_MEMORY_D0"]);
            (board.resetPin, board.resetGate) = Sense("U1.RESET_RELEASE", board.rail);
            board.portPins = new SparkTerminal[6];
            board.portGates = new SparkSwitch[6];
            board.portMarkers = new Renderer[6];
            for (int i = 0; i < 6; i++)
            {
                int port = i + 1;
                (board.portPins[i], board.portGates[i]) = Sense($"J{port}.LINK_RETURN", routeWitnesses[$"J{port}.PHY_RETURN"]);
                var point = Site("J" + port);
                var marker = Marker(graph.Root, $"J{port} independent port status", materials["status_blue"], .0055f);
                marker.transform.localPosition = new Vector3(.193f, .009f, point.z);
                board.portMarkers[i] = marker;
                BoardText(graph.Root, $"J{port}  /  {(i < 2 ? $"PATH {(i == 0 ? "A" : "B")}" : "FIELD")}",
                    new Vector3(.178f, .0018f, point.z - .012f), .00077f, Color.white);
            }

            Vector3 a = FindPad(Site("U31"), 6, pads);
            Vector3 b = FindPad(Site("U32"), 2, pads);
            var sender = graph.Pin("U31.TRUSTED_DETERMINISTIC_READY_OUT", a, true);
            var receiver = graph.Pin("U32.TRUSTED_DETERMINISTIC_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U31 deterministic-ready heartbeat", board.monitorSupply, sender);
            var pullDown = graph.Load("U32 independent feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel118Process.Route
            {
                id = "W321", group = "Independent supervision", net = "TRUSTED_DETERMINISTIC_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["gold"], "W321", a, b)
            };
            map.Add(Row(board.jumper));
            (board.protectedFeed, board.workload) = Sense("U32.PROTECTED_OUTPUT_PERMIT", board.monitorSupply);
            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W321 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["gold"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["gold"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["status_blue"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["status_blue"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U32 protected-output indicator", PrimitiveType.Cube,
                b + new Vector3(.006f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["status_green"]);
            board.faultOverlays = new[] { Site("Y1"), board.routes[board.pathViaIndex].destination, Site("U15") }
                .Select((point, i) =>
                {
                    var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["alert_red"], .012f);
                    marker.transform.localPosition = point + Vector3.up * .013f;
                    marker.enabled = true;
                    marker.gameObject.SetActive(false);
                    return marker.gameObject;
                }).ToArray();
            BoardText(graph.Root, "TSX-26  /  THE DETERMINISTIC GRID", new Vector3(0, .0017f, -.155f), .00132f, Color.white);
            BoardText(graph.Root, "W321  /  U32", (a + b) * .5f + Vector3.right * .022f, .00091f, Color.white);
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

        private static string Row(SparkLevel118Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000f:F3},{route.entry.z * 1000f:F3},{route.destination.x * 1000f:F3},{route.destination.z * 1000f:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int start, List<Vector3> occupied)
        {
            for (int offset = 0; offset < 128; offset++)
            {
                int step = (start + offset) % 8;
                float angle = step * Mathf.PI * .25f;
                float radius = .0068f + (offset / 8) * .0025f;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .233f || Mathf.Abs(pad.z) > .163f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free TSX26-EDU-1 contact at " + near);
        }
    }
}
