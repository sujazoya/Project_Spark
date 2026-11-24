using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.RedundancyCitadel;
using UnityEngine;

namespace ProjectSpark.EditorTools.RedundancyCitadel
{
    internal static partial class SparkLevel113Builder
    {
        private static List<string> ConfigureGraph(SparkLevel113Process board, Circuit graph, string[][] plans,
            IEnumerable<string> viaIds, Dictionary<string, Material> materials)
        {
            Vector3 Site(string label) => graph.Root.InverseTransformPoint(
                board.parts.Single(part => part.label == label).footprint.position);
            board.ground = graph.Pin("SUPPLY_RET");
            board.input = graph.Pin("SUPPLY_POS");
            board.reserveInput = graph.Pin("RESERVE_POS");
            board.source = graph.Supply("PRIMARY_12V", board.input, board.ground, 12.15f);
            board.reserve = graph.Supply("RESERVE_12V", board.reserveInput, board.ground, 11.85f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            board.bus = graph.Pin("OR_BUS");
            var primaryFeed = graph.Pin("Q1.PRIMARY_OR_INPUT");
            var reserveFeed = graph.Pin("Q2.RESERVE_OR_INPUT");
            board.primaryPath = graph.SwitchedOutput("Q1 primary OR-ing path", board.input, primaryFeed);
            board.reservePath = graph.SwitchedOutput("Q2 reserve takeover", board.reserveInput, reserveFeed);
            board.primaryIsolation = graph.Isolation("Primary reverse isolation", primaryFeed, board.bus);
            board.reserveIsolation = graph.Isolation("Reserve reverse isolation", reserveFeed, board.bus);
            // The native graph discovers unwired nodes from electrical-device hierarchies.
            board.bus.transform.SetParent(board.primaryIsolation.transform, true);
            board.busLoad = graph.Load("Transfer qualification load", board.bus, board.ground, 30f);
            (board.rail5, board.enable5) = Regulate("PROTECTED_5V", 5f);
            (board.rail33, board.enable33) = Regulate("PROTECTED_3V3", 3.3f);
            board.monitorSupply = graph.Pin("U6.DIAGNOSTICS_3V3");
            var diagnosticFeed = graph.Load("Independent diagnostic supply feed", board.rail33, board.monitorSupply, .35f);
            board.monitorSupply.transform.SetParent(diagnosticFeed.transform, true);
            graph.Load("U6 monitor load", board.monitorSupply, board.ground, 1200f);
            board.protectedFeed = graph.Pin("U6.PROTECTED_LOAD");
            board.workload = graph.SwitchedOutput("U6 safety relay", board.rail33, board.protectedFeed);
            graph.Load("Protected demonstration load", board.protectedFeed, board.ground, 47f);

            var routes = new List<SparkLevel113Process.Route>(SparkLevel113Process.RouteCount);
            var witnesses = new List<SparkTerminal>();
            var returns = new List<SparkTerminal>();
            var availableVias = new Queue<string>(viaIds);
            var pads = new List<Vector3>();
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var map = new List<string> { "task,domain,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                int domain = witnesses.Count + 1;
                string domainId = $"D{domain:00}";
                var previous = graph.Pin(domainId + ".TEST_POS");
                var returnPin = graph.Pin(domainId + ".TEST_RET");
                returns.Add(returnPin);
                var source = graph.Supply(domainId + " isolated continuity source", previous, returnPin, 5f);
                returnPin.transform.SetParent(source.transform, true);
                Set(returnPin, ("owner", source));
                int quota = domain <= 4 ? 10 : 9;
                int groupIndex = 0;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    bool eccSpan = group.Key.StartsWith("ECC", StringComparison.Ordinal) && groupIndex == 4;
                    string net = eccSpan ? "U7.ECC_DQ_TEST" : domainId + ".CONTINUITY";
                    Vector3 site = eccSpan ? Site("U7") : sites[(index * 37) % sites.Length];
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var route = new SparkLevel113Process.Route
                    {
                        id = row[0], group = group.Key, net = net,
                        from = graph.Pin(row[0] + ".A", start, true), to = graph.Pin(row[0] + ".B", end, true),
                        entry = start, destination = end, layer = index % SparkLevel113Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[eccSpan ? "Gold_ENIG" : "Copper_Exposed"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " continuity link", previous, route.from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = route.to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel113Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", route.to, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.0022f, .0002f, .0022f), materials["Tin_Solder"]);
                        route.viaVisual.enabled = false;
                    }
                    if (eccSpan)
                    {
                        board.eccRoute = index;
                        board.eccDomain = witnesses.Count;
                    }
                    groupIndex++;
                    routes.Add(route);
                    map.Add(Row(route));
                }
                graph.Load(group.Key + " continuity witness", previous, returnPin, 1300f);
                witnesses.Add(previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel113Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel113Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel113Process.LayerCount ||
                board.parts.Any(part => !part.contact) || !routes[board.eccRoute].barrel)
                throw new InvalidOperationException("The RDX21-EDU-1 topology is incomplete.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
            board.continuityReturns = returns.ToArray();

            Vector3 a = FindPad(Site("U15"), 6, pads);
            Vector3 b = FindPad(Site("U6"), 2, pads);
            var sender = graph.Pin("U15.TRUSTED_FAILOVER_READY_OUT", a, true);
            var receiver = graph.Pin("U6.TRUSTED_FAILOVER_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U15 authorization signal", board.monitorSupply, sender);
            var pullDown = graph.Load("U6 authorization pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel113Process.Route
            {
                id = "W233", group = "Independent safety", net = "TRUSTED_FAILOVER_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["Gold_ENIG"], "W233", a, b)
            };
            map.Add(Row(board.jumper));
            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W233 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["Gold_ENIG"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["Gold_ENIG"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["Signal_Cyan"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["Signal_Cyan"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U6 interlock indicator", PrimitiveType.Cube,
                b + new Vector3(.005f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["Signal_Cyan"]);
            board.faultOverlays = new[] { Site("Q1"), board.routes[board.eccRoute].destination, Site("Y1") }.Select((point, i) =>
            {
                var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["Fault_Red"], .010f);
                marker.transform.localPosition = point + Vector3.up * .012f;
                marker.enabled = true;
                marker.gameObject.SetActive(false);
                return marker.gameObject;
            }).ToArray();
            BoardText(graph.Root, "RDX-21  /  REDUNDANCY CITADEL", new Vector3(.003f, .0012f, -.130f), .00125f, Color.white);
            BoardText(graph.Root, "U6  /  INDEPENDENT SUPERVISOR", b + new Vector3(.015f, .0003f, -.012f), .00058f, Color.white);
            BoardText(graph.Root, "U15  /  W233 AUTHORIZATION", a + new Vector3(.023f, .0003f, .006f), .00060f, Color.white);
            WorkLight(graph.Root, "Soft warm key", new Vector3(-.105f, .140f, -.050f), new Color(1f, .91f, .82f), 11f);
            WorkLight(graph.Root, "Cool fill", new Vector3(.144f, .139f, .058f), new Color(.70f, .88f, 1f), 10f);
            return map;

            (SparkTerminal, SparkSwitch) Regulate(string name, float volts)
            {
                var supplyPin = graph.Pin(name + ".SOURCE");
                graph.Supply(name + " training regulator", supplyPin, board.ground, volts);
                var rail = graph.Pin(name);
                var enable = graph.SwitchedOutput(name + " enable", supplyPin, rail);
                graph.Load(name + " sample load", rail, board.ground, 100f);
                return (rail, enable);
            }
        }

        private static string Row(SparkLevel113Process.Route route) => FormattableString.Invariant(
            $"{route.id},\"{route.group.Replace("\"", "\"\"")}\",{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int offset, List<Vector3> occupied)
        {
            for (int ring = 0; ring < 12; ring++)
            for (int notch = 0; notch < 8; notch++)
            {
                float angle = (offset + notch) * Mathf.PI / 4f;
                float radius = .0052f + .0024f * ring;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .185f || Mathf.Abs(pad.z) > .130f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000035f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free RDX21-EDU-1 terminal at " + near);
        }
    }
}
