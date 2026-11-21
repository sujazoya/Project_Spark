using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.DataPhoenix;
using UnityEngine;

namespace ProjectSpark.EditorTools.DataPhoenix
{
    internal static partial class SparkLevel116Builder
    {
        private static List<string> ConfigureGraph(SparkLevel116Process board, Circuit graph, string[][] plans,
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
            board.bench = graph.SwitchedOutput("Primary storage feed", board.input, board.standby);
            graph.Load("Primary feed witness", board.standby, board.ground, 1000f);
            (board.aux, board.auxGate) = Regulate("U30.DIAGNOSTIC_3V3", 3.3f);
            (board.controllerRail, board.controllerGate) = Regulate("U1.CONTROLLER_1V2", 1.2f);
            (board.nandRail, board.nandGate) = Regulate("NAND.ARRAY_1V8", 1.8f);
            board.reserveSense = graph.Pin("U2.RESERVE_CHARGE_WITNESS");
            board.reserveCharge = graph.SwitchedOutput("PLP bank charge", board.aux, board.reserveSense);
            graph.Load("PLP recharge sense", board.reserveSense, board.ground, 1200f);

            var routes = new List<SparkLevel116Process.Route>(SparkLevel116Process.RouteCount);
            var witnesses = new List<SparkTerminal>();
            var availableVias = new Queue<string>(viaIds);
            var pads = new List<Vector3>();
            var viaOutputs = new Dictionary<int, SparkTerminal>();
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var map = new List<string> { "task,domain,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                var previous = board.input;
                int quota = witnesses.Count < 4 ? 17 : 16;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = index switch
                    {
                        0 => "U1.POWER_WITNESS", 36 => "U3.TRANSPORT_CLOCK",
                        >= 64 and < 68 => $"NAND{index - 63}.CE_WITNESS",
                        >= 124 and < 128 => $"NAND{index - 119}.CE_WITNESS",
                        184 => "NAND8.JOURNAL_WITNESS", 232 => "PLP3.BANK_WITNESS", _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 37) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel116Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + (witnesses.Count + 1),
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel116Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "Copper_Trace" : "Gold_ENIG"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity link", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel116Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        viaOutputs.Add(index, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.00225f, .0002f, .00225f), materials["Solder_Silver"]);
                        route.viaVisual.enabled = false;
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                }
                graph.Load(group.Key + " continuity witness", previous, board.ground, 1300f);
                witnesses.Add(previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel116Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel116Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel116Process.LayerCount ||
                board.parts.Any(part => !part.contact))
                throw new InvalidOperationException("The NVX24-EDU-1 topology is incomplete.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
            board.powerWitness = routes[0].to;
            board.bankWitness = viaOutputs[232];
            board.linkSense = graph.Pin("U1.NVME_LINK_WITNESS");
            board.linkContact = graph.SwitchedOutput("U3 transport clock/reset witness", viaOutputs[36], board.linkSense);
            graph.Load("Educational transport receiver", board.linkSense, board.ground, 10000f);
            board.channelViaIndex = 67;
            board.channelFeeds = new SparkTerminal[8];
            board.channelContacts = new SparkSwitch[8];
            for (int channel = 0; channel < 8; channel++)
            {
                int index = channel < 4 ? 64 + channel : 120 + channel;
                var sense = graph.Pin($"NAND{channel + 1}.CE_READY_{channel}");
                board.channelFeeds[channel] = sense;
                board.channelContacts[channel] = graph.SwitchedOutput(
                    routes[index].viaId + $" channel {channel} CE contact", viaOutputs[index], sense);
                graph.Load($"NAND channel {channel} read witness", sense, board.ground, 20000f);
            }

            Vector3 a = FindPad(Site("U31"), 6, pads);
            Vector3 b = FindPad(Site("U30"), 2, pads);
            var sender = graph.Pin("U31.TRUSTED_STORAGE_READY_OUT", a, true);
            var receiver = graph.Pin("U30.TRUSTED_STORAGE_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U31 verified storage heartbeat", board.aux, sender);
            var pullDown = graph.Load("U30 feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel116Process.Route
            {
                id = "W289", group = "Independent protection", net = "TRUSTED_STORAGE_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["Gold_ENIG"], "W289", a, b)
            };
            map.Add(Row(board.jumper));
            board.monitorSupply = graph.Pin("U30.INDEPENDENT_3V3");
            var supervisor = graph.Load("U30 independent retained feed", board.aux, board.monitorSupply, .35f);
            board.monitorSupply.transform.SetParent(supervisor.transform, true);
            graph.Load("Independent supervisor bias", board.monitorSupply, board.ground, 1500f);
            board.protectedFeed = graph.Pin("U30.PROTECTED_WRITE_PERMIT");
            board.workload = graph.SwitchedOutput("Protected write interlock", board.monitorSupply, board.protectedFeed);
            graph.Load("Protected write witness", board.protectedFeed, board.ground, 90f);

            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W289 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["Gold_ENIG"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["Gold_ENIG"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["Silkscreen_Cyan"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["Silkscreen_Cyan"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U30 write permit indicator", PrimitiveType.Cube,
                b + new Vector3(.005f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["LED_Green"]);
            board.faultOverlays = new[] { Site("NAND4"), Site("PLP3"), Site("NAND8") }.Select((point, i) =>
            {
                var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["LED_Red"], .011f);
                marker.transform.localPosition = point + Vector3.up * (i == 1 ? .035f : .016f);
                marker.enabled = true;
                marker.gameObject.SetActive(false);
                return marker.gameObject;
            }).ToArray();
            board.channelMarkers = Enumerable.Range(1, 8).Select(i =>
            {
                var point = Site("NAND" + i);
                var marker = Marker(graph.Root, $"Channel {i - 1} status", materials["Silkscreen_Cyan"], .006f);
                marker.transform.localPosition = point + new Vector3(.014f, .012f, .013f);
                return marker;
            }).ToArray();
            BoardText(graph.Root, "NVX-24  /  THE DATA PHOENIX", new Vector3(0, .0017f, -.144f), .00127f, Color.white);
            for (int channel = 0; channel < 8; channel++)
            {
                var point = Site("NAND" + (channel + 1));
                BoardText(graph.Root, $"CH {channel}", point + new Vector3(0, .0008f, .018f), .0008f, Color.white);
            }
            BoardText(graph.Root, "PLP  /  HOLD-UP RESERVE", Site("PLP3") + new Vector3(0, .0006f, .027f), .00094f, Color.white);
            BoardText(graph.Root, "W289  /  U30 SUPERVISOR", (a + b) * .5f + Vector3.forward * .009f, .0007f, Color.white);
            WorkLight(graph.Root, "Neutral task light", new Vector3(-.115f, .149f, -.052f), new Color(1f, .94f, .86f), 8.5f);
            WorkLight(graph.Root, "Cool storage fill", new Vector3(.143f, .127f, .063f), new Color(.64f, .88f, 1f), 8.7f);
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

        private static string Row(SparkLevel116Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int offset, List<Vector3> occupied)
        {
            for (int ring = 0; ring < 12; ring++)
            for (int notch = 0; notch < 8; notch++)
            {
                float angle = (offset + notch) * Mathf.PI / 4f;
                float radius = .0054f + .0024f * ring;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .214f || Mathf.Abs(pad.z) > .148f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free NVX24-EDU-1 terminal at " + near);
        }
    }
}
