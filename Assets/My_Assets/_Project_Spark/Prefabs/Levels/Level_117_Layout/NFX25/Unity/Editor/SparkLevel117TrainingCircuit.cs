using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.PacketCitadel;
using UnityEngine;

namespace ProjectSpark.EditorTools.PacketCitadel
{
    internal static partial class SparkLevel117Builder
    {
        private static List<string> ConfigureGraph(SparkLevel117Process board, Circuit graph, string[][] plans,
            IEnumerable<string> viaIds, Dictionary<string, Material> materials)
        {
            Vector3 Site(string reference) => graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == reference).footprint.position);
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 5f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity test return", board.input, board.ground, 10000f);
            board.standby = graph.Pin("U2.PRIMARY_5V");
            board.bench = graph.SwitchedOutput("Primary switch feed", board.input, board.standby);
            graph.Load("Primary feed witness", board.standby, board.ground, 1000f);
            (board.aux, board.auxGate) = Regulate("U36.DIAGNOSTIC_3V3", 3.3f);
            (board.coreRail, board.coreGate) = Regulate("U1.CORE_1V2", 1.2f);
            (board.moduleRail, board.moduleGate) = Regulate("SFP.MODULE_3V3", 3.3f);
            board.resetSense = graph.Pin("U1.RESET_RELEASE");
            board.resetContact = graph.SwitchedOutput("Switch reset release", board.aux, board.resetSense);
            graph.Load("Reset pull-down", board.resetSense, board.ground, 10000f);

            var routes = new List<SparkLevel117Process.Route>(SparkLevel117Process.RouteCount);
            var witnesses = new List<SparkTerminal>();
            var availableVias = new Queue<string>(viaIds);
            var pads = new List<Vector3>();
            var viaOutputs = new Dictionary<int, SparkTerminal>();
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var map = new List<string> { "task,domain,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                var previous = board.input;
                int quota = witnesses.Count < 3 ? 16 : 15;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = index switch
                    {
                        0 => "U1.POWER_WITNESS", 44 => "Y1.REFERENCE_CLOCK", 45 => "Y2.DISCIPLINE_WITNESS",
                        >= 88 and < 92 => $"U{index - 85}.SERDES_LANE",
                        >= 132 and < 136 => $"U{index - 129}.SFP_BIAS_WITNESS", _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 37) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel117Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + (witnesses.Count + 1),
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel117Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "Copper_Amber" : "Gold_ENIG"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity link", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel117Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        viaOutputs.Add(index, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.00225f, .0002f, .00225f), materials["Solder_Tin"]);
                        route.viaVisual.enabled = false;
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                }
                graph.Load(group.Key + " continuity witness", previous, board.ground, 1300f);
                witnesses.Add(previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel117Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel117Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel117Process.LayerCount ||
                board.parts.Any(part => !part.contact))
                throw new InvalidOperationException("The NFX25-EDU-1 topology is incomplete.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
            board.powerWitness = routes[0].to;
            board.clock = graph.Pin("U2.CLOCK_125MHZ_VALID");
            board.clockContact = graph.SwitchedOutput("Y1 reference clock witness", viaOutputs[44], board.clock);
            graph.Load("Reference clock input", board.clock, board.ground, 20000f);
            board.timingBias = graph.Pin("Y2.STABLE_REFERENCE");
            board.timingContact = graph.SwitchedOutput("Y2 stabilized clock bias", viaOutputs[45], board.timingBias);
            graph.Load("Oscillator discipline bias witness", board.timingBias, board.ground, 20000f);

            board.laneViaIndex = 91;
            board.moduleFeeds = new SparkTerminal[4];
            board.biasFeeds = new SparkTerminal[4];
            board.laneFeeds = new SparkTerminal[4];
            board.portWitnesses = new SparkTerminal[4];
            board.portContacts = new SparkSwitch[4];
            board.biasContacts = new SparkSwitch[4];
            board.laneContacts = new SparkSwitch[4];
            for (int port = 0; port < 4; port++)
            {
                int number = port + 1;
                board.portWitnesses[port] = viaOutputs[132 + port];
                var feed = graph.Pin($"P{number}.MODULE_POWER");
                board.moduleFeeds[port] = feed;
                board.portContacts[port] = graph.SwitchedOutput($"P{number} emitter power", board.moduleRail, feed);
                graph.Load($"P{number} module load", feed, board.ground, 470f);
                var bias = graph.Pin($"P{number}.TX_BIAS");
                board.biasFeeds[port] = bias;
                board.biasContacts[port] = graph.SwitchedOutput($"P{number} transmitter bias contact", feed, bias);
                graph.Load($"P{number} bias return", bias, board.ground, 1800f);
                var lane = graph.Pin($"P{number}.LANE_WITNESS");
                board.laneFeeds[port] = lane;
                board.laneContacts[port] = graph.SwitchedOutput(
                    routes[88 + port].viaId + $" P{number} escape continuity", viaOutputs[88 + port], lane);
                graph.Load($"P{number} SerDes monitor", lane, board.ground, 15000f);
            }

            Vector3 a = FindPad(Site("U35"), 6, pads);
            Vector3 b = FindPad(Site("U36"), 2, pads);
            var sender = graph.Pin("U35.QUALIFIED_NETWORK_READY_OUT", a, true);
            var receiver = graph.Pin("U36.QUALIFIED_NETWORK_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U35 verified link heartbeat", board.aux, sender);
            var pullDown = graph.Load("U36 feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel117Process.Route
            {
                id = "W305", group = "Independent protection", net = "QUALIFIED_NETWORK_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["Gold_ENIG"], "W305", a, b)
            };
            map.Add(Row(board.jumper));
            board.monitorSupply = graph.Pin("U36.INDEPENDENT_3V3");
            var supervisor = graph.Load("U36 independent retained feed", board.aux, board.monitorSupply, .35f);
            board.monitorSupply.transform.SetParent(supervisor.transform, true);
            graph.Load("Independent supervisor bias", board.monitorSupply, board.ground, 1500f);
            board.protectedFeed = graph.Pin("U36.PROTECTED_NETWORK_PERMIT");
            board.workload = graph.SwitchedOutput("Protected network interlock", board.monitorSupply, board.protectedFeed);
            graph.Load("Protected network witness", board.protectedFeed, board.ground, 90f);

            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W305 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["Gold_ENIG"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["Gold_ENIG"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["Silkscreen_Cyan"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["Silkscreen_Cyan"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U36 write permit indicator", PrimitiveType.Cube,
                b + new Vector3(.005f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["LED_Emerald"]);
            board.faultOverlays = new[] { Site("U5"), Site("Y2"), Site("U6") }.Select((point, i) =>
            {
                var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["LED_Crimson"], .012f);
                marker.transform.localPosition = point + Vector3.up * .013f;
                marker.enabled = true;
                marker.gameObject.SetActive(false);
                return marker.gameObject;
            }).ToArray();
            board.portMarkers = Enumerable.Range(0, 4).Select(port =>
            {
                var point = Site("U" + (port + 3));
                var marker = Marker(graph.Root, $"P{port + 1} optical witness", materials["Silkscreen_Cyan"], .006f);
                marker.transform.localPosition = new Vector3(.163f, .008f, point.z + .017f);
                return marker;
            }).ToArray();
            for (int port = 0; port < 4; port++)
            {
                var point = Site("U" + (port + 3));
                BoardText(graph.Root, $"P{port + 1}  /  OPTICAL", new Vector3(.136f, .00175f, point.z - .023f), .00088f, Color.white);
            }
            BoardText(graph.Root, "NFX-25  /  THE PACKET CITADEL", new Vector3(0, .0017f, -.147f), .00128f, Color.white);
            BoardText(graph.Root, "Y2  /  PTP REFERENCE", Site("Y2") + new Vector3(0, .0006f, .024f), .0009f, Color.white);
            BoardText(graph.Root, "W305  /  U36", (a + b) * .5f + Vector3.right * .022f, .00075f, Color.white);
            WorkLight(graph.Root, "Neutral task light", new Vector3(-.115f, .149f, -.052f), new Color(1f, .94f, .86f), 7.1f);
            WorkLight(graph.Root, "Cool optical fill", new Vector3(.143f, .127f, .063f), new Color(.64f, .88f, 1f), 6.75f);
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

        private static string Row(SparkLevel117Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int offset, List<Vector3> occupied)
        {
            for (int ring = 0; ring < 12; ring++)
            for (int notch = 0; notch < 8; notch++)
            {
                float angle = (offset + notch) * Mathf.PI / 4f;
                float radius = .0054f + .0024f * ring;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .224f || Mathf.Abs(pad.z) > .153f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free NFX25-EDU-1 terminal at " + near);
        }
    }
}
