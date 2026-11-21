using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.ThermalDominion;
using UnityEngine;

namespace ProjectSpark.EditorTools.ThermalDominion
{
    internal static partial class SparkLevel114Builder
    {
        private static List<string> ConfigureGraph(SparkLevel114Process board, Circuit graph, string[][] plans,
            IEnumerable<string> viaIds, Dictionary<string, Material> materials)
        {
            Vector3 Site(string label) => graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == label).footprint.position);
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 5f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity test return", board.input, board.ground, 10000f);
            board.standby = graph.Pin("TDX22.PROTECTED_5V");
            board.bench = graph.SwitchedOutput("Bench isolation", board.input, board.standby);
            graph.Load("Protected rail witness", board.standby, board.ground, 1000f);
            (board.aux, board.auxGate) = Regulate("U7.AUX_3V3", 3.3f);
            (board.cpuRail, board.cpuGate) = Regulate("U1.CORE_1V05", 1.05f);
            (board.acceleratorRail, board.acceleratorGate) = Regulate("U2.CORE_1V15", 1.15f);
            var vrmRail = graph.Pin("Q_BANK.LOAD_FEED");
            board.vrmGate = graph.SwitchedOutput("VRM diagnostic load switch", board.standby, vrmRail);
            board.cpuLoad = graph.Load("U1 load profile", board.cpuRail, board.ground, 5.8f);
            board.acceleratorLoad = graph.Load("U2 load profile", board.acceleratorRail, board.ground, 8.2f);
            board.vrmLoad = graph.Load("VRM load profile", vrmRail, board.ground, 180f);
            board.fanSense = graph.Pin("FAN1.PWM_DRIVE");
            board.fanDrive = graph.SwitchedOutput("U8 fan driver enable", board.aux, board.fanSense);
            graph.Load("Fan driver witness", board.fanSense, board.ground, 820f);

            var routes = new List<SparkLevel114Process.Route>(SparkLevel114Process.RouteCount);
            var witnesses = new List<SparkTerminal>();
            var availableVias = new Queue<string>(viaIds);
            var pads = new List<Vector3>();
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var map = new List<string> { "task,domain,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                var previous = board.input;
                int quota = witnesses.Count < 4 ? 13 : 12;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = index switch
                    {
                        0 => "U1.CPU_FEED", 1 => "U2.ACCELERATOR_FEED", 44 => "Q1.VRM_CONTINUITY",
                        >= 82 and <= 89 => $"TS{index - 81}.THERMOMETRY",
                        124 => "U8.FAN_COMMAND", _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 37) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel114Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + (witnesses.Count + 1),
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel114Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "Copper_Bright" : "Gold_ENIG"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " continuity link", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota > 0)
                    {
                        quota--;
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel114Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.0022f, .0002f, .0022f), materials["Tin_Solder"]);
                        route.viaVisual.enabled = false;
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                }
                graph.Load(group.Key + " continuity witness", previous, board.ground, 1300f);
                witnesses.Add(previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel114Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel114Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel114Process.LayerCount ||
                board.parts.Any(part => !part.contact))
                throw new InvalidOperationException("The TDX22-EDU-1 topology is incomplete.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
            board.channelFeeds = new[] { 0, 1, 44, 124 }.Select(index => routes[index].to).ToArray();
            board.sensorFeeds = routes.Skip(82).Take(8).Select(route => route.to).ToArray();
            board.thermalViaIndex = 44;

            Vector3 a = FindPad(Site("U11"), 6, pads);
            Vector3 b = FindPad(Site("U10"), 2, pads);
            var sender = graph.Pin("U11.SAFE_THERMAL_HEALTH_OUT", a, true);
            var receiver = graph.Pin("U10.SAFE_THERMAL_HEALTH_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U11 thermal heartbeat", board.aux, sender);
            var pullDown = graph.Load("U10 feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel114Process.Route
            {
                id = "W257", group = "Independent protection", net = "SAFE_THERMAL_HEALTH", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["Gold_ENIG"], "W257", a, b)
            };
            map.Add(Row(board.jumper));
            board.monitorSupply = graph.Pin("U10.INDEPENDENT_3V3");
            var supervisor = graph.Load("U10 independent feed", board.aux, board.monitorSupply, .35f);
            board.monitorSupply.transform.SetParent(supervisor.transform, true);
            graph.Load("Independent supervisor bias", board.monitorSupply, board.ground, 1500f);
            board.protectedFeed = graph.Pin("U10.PROTECTED_WORKLOAD");
            board.workload = graph.SwitchedOutput("Protected workload relay", board.monitorSupply, board.protectedFeed);
            graph.Load("Protected workload test load", board.protectedFeed, board.ground, 90f);

            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W257 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["Gold_ENIG"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["Gold_ENIG"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["Indicator_Cyan"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["Indicator_Cyan"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U10 workload indicator", PrimitiveType.Cube,
                b + new Vector3(.005f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["Indicator_Cyan"]);
            board.faultOverlays = new[] { Site("HS1"), Site("FAN1"), Site("Q1") }.Select((point, i) =>
            {
                var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["Indicator_Red"], .010f);
                marker.transform.localPosition = point + Vector3.up * (i == 0 ? .026f : .017f);
                marker.enabled = true;
                marker.gameObject.SetActive(false);
                return marker.gameObject;
            }).ToArray();
            board.thermalMarkers = Enumerable.Range(1, 8).Select(i =>
            {
                var marker = Marker(graph.Root, $"TS{i} measured zone", materials["Indicator_Cyan"], .0072f);
                marker.transform.localPosition = Site("TS" + i) + Vector3.up * .009f;
                return marker;
            }).ToArray();
            BoardText(graph.Root, "TDX-22  /  THE THERMAL DOMINION", new Vector3(.003f, .0012f, -.123f), .00125f, Color.white);
            BoardText(graph.Root, "U1  /  HS1", Site("U1") + new Vector3(0, -.0008f, -.050f), .001f, Color.white);
            BoardText(graph.Root, "U2  /  HS2", Site("U2") + new Vector3(0, -.0008f, -.050f), .001f, Color.white);
            BoardText(graph.Root, "W257  /  U10 SUPERVISOR", (a + b) * .5f + Vector3.forward * .007f, .00065f, Color.white);
            WorkLight(graph.Root, "Soft warm key", new Vector3(-.105f, .140f, -.050f), new Color(1f, .91f, .82f), 11f);
            WorkLight(graph.Root, "Cool fill", new Vector3(.144f, .139f, .058f), new Color(.70f, .88f, 1f), 10f);
            return map;

            (SparkTerminal, SparkSwitch) Regulate(string name, float volts)
            {
                var supplyPin = graph.Pin(name + ".SOURCE");
                graph.Supply(name + " training regulator", supplyPin, board.ground, volts);
                var rail = graph.Pin(name);
                var enable = graph.SwitchedOutput(name + " enable", supplyPin, rail);
                graph.Load(name + " sample load", rail, board.ground, 1200f);
                return (rail, enable);
            }
        }

        private static string Row(SparkLevel114Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int offset, List<Vector3> occupied)
        {
            for (int ring = 0; ring < 12; ring++)
            for (int notch = 0; notch < 8; notch++)
            {
                float angle = (offset + notch) * Mathf.PI / 4f;
                float radius = .0052f + .0024f * ring;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .194f || Mathf.Abs(pad.z) > .133f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000035f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free TDX22-EDU-1 terminal at " + near);
        }
    }
}
