using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.SecureEnclave;
using UnityEngine;

namespace ProjectSpark.EditorTools.SecureEnclave
{
    internal static partial class SparkLevel115Builder
    {
        private static List<string> ConfigureGraph(SparkLevel115Process board, Circuit graph, string[][] plans,
            IEnumerable<string> viaIds, Dictionary<string, Material> materials)
        {
            Vector3 Site(string reference) => graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == reference).footprint.position);
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 5f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity test return", board.input, board.ground, 10000f);
            board.standby = graph.Pin("U11.PROTECTED_5V");
            board.bench = graph.SwitchedOutput("Bench isolation", board.input, board.standby);
            graph.Load("Protected rail witness", board.standby, board.ground, 1000f);
            (board.aux, board.auxGate) = Regulate("U13.DIAGNOSTIC_3V3", 3.3f);
            (board.secure, board.secureContact) = Regulate("U2.SECURE_1V8", 1.8f);
            board.clock = graph.Pin("U6.CLOCK_VALID");
            board.clockGate = graph.SwitchedOutput("Secure clock validity", board.aux, board.clock);
            graph.Load("Clock monitor input", board.clock, board.ground, 10000f);
            board.reset = graph.Pin("U15.RESET_RELEASE");
            board.resetGate = graph.SwitchedOutput("Power-on reset release", board.aux, board.reset);
            graph.Load("Reset hold bias", board.reset, board.ground, 10000f);

            var routes = new List<SparkLevel115Process.Route>(SparkLevel115Process.RouteCount);
            var witnesses = new List<SparkTerminal>();
            var availableVias = new Queue<string>(viaIds);
            var pads = new List<Vector3>();
            var viaOutputs = new Dictionary<int, SparkTerminal>();
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var map = new List<string> { "task,domain,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                var previous = board.input;
                int quota = witnesses.Count == 0 ? 14 : 13;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = index switch
                    {
                        0 => "Q1.SECURE_FEED", 36 => "Y1.CLOCK_WITNESS",
                        68 => "U4.BOOT_IMAGE_DATA", 122 => "U10.SHIELD_RETURN", _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 37) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel115Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + (witnesses.Count + 1),
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel115Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "Copper_Trace" : "Gold_ENIG"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity link", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel115Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        viaOutputs.Add(index, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.0023f, .0002f, .0023f), materials["Solder_Silver"]);
                        route.viaVisual.enabled = false;
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                }
                graph.Load(group.Key + " continuity witness", previous, board.ground, 1300f);
                witnesses.Add(previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel115Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel115Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel115Process.LayerCount ||
                board.parts.Any(part => !part.contact))
                throw new InvalidOperationException("The SHX23-EDU-1 topology is incomplete.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
            board.railWitness = routes[0].to;
            board.clockWitness = routes[36].to;
            board.flashViaIndex = 68;
            board.flash = graph.Pin("U4.READ_CONTINUITY");
            board.flashContact = graph.SwitchedOutput(routes[68].viaId + " longitudinal flash contact", viaOutputs[68], board.flash);
            graph.Load("Flash read monitor", board.flash, board.ground, 10000f);

            var meshNode = graph.Pin("U10.MESH_DRIVE");
            graph.SwitchedOutput("Tamper mesh feed", board.aux, meshNode, true);
            board.shieldSeams = new SparkSwitch[board.shieldParts.Length];
            for (int i = 0; i < board.shieldSeams.Length; i++)
            {
                var next = graph.Pin($"SH{i + 1}.SEAM_RETURN");
                board.shieldSeams[i] = graph.SwitchedOutput($"SH{i + 1} tamper seam", meshNode, next);
                meshNode = next;
            }
            board.meshSense = meshNode;
            graph.Load("U10 independent tamper pull-down", board.meshSense, board.ground, 10000f);

            Vector3 a = FindPad(Site("U7"), 6, pads);
            Vector3 b = FindPad(Site("U8"), 2, pads);
            var sender = graph.Pin("U7.TRUST_READY_OUT", a, true);
            var receiver = graph.Pin("U8.TRUST_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U7 authenticated boot heartbeat", board.aux, sender);
            var pullDown = graph.Load("U8 feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel115Process.Route
            {
                id = "W273", group = "Independent protection", net = "TRUST_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["Gold_ENIG"], "W273", a, b)
            };
            map.Add(Row(board.jumper));
            board.monitorSupply = graph.Pin("U8.INDEPENDENT_3V3");
            var supervisor = graph.Load("U8 independent feed", board.aux, board.monitorSupply, .35f);
            board.monitorSupply.transform.SetParent(supervisor.transform, true);
            graph.Load("Independent supervisor bias", board.monitorSupply, board.ground, 1500f);
            board.protectedFeed = graph.Pin("U8.TRUSTED_WORKLOAD");
            board.workload = graph.SwitchedOutput("Protected workload interlock", board.monitorSupply, board.protectedFeed);
            graph.Load("Protected workload witness", board.protectedFeed, board.ground, 90f);

            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W273 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["Gold_ENIG"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["Gold_ENIG"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["Status_Cyan"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["Status_Cyan"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U8 workload indicator", PrimitiveType.Cube,
                b + new Vector3(.005f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["Status_Cyan"]);
            board.faultOverlays = new[] { Site("Q1"), Site("U4"), Site("SH3") }.Select((point, i) =>
            {
                var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["Status_Red"], .010f);
                marker.transform.localPosition = point + Vector3.up * (i > 0 ? .016f : .008f);
                marker.enabled = true;
                marker.gameObject.SetActive(false);
                return marker.gameObject;
            }).ToArray();
            BoardText(graph.Root, "SHX-23  /  THE SECURE ENCLAVE", new Vector3(0, .0016f, -.131f), .00132f, Color.white);
            BoardText(graph.Root, "ROOT OF TRUST", Site("U1") + new Vector3(0, .0006f, .036f), .00095f, Color.white);
            BoardText(graph.Root, "SECURE ELEMENT", Site("U2") + new Vector3(0, .0006f, .036f), .00095f, Color.white);
            BoardText(graph.Root, "SIGNED IMAGE", Site("U4") + new Vector3(0, .0006f, -.035f), .00095f, Color.white);
            BoardText(graph.Root, "W273  /  U8 SUPERVISOR", (a + b) * .5f + Vector3.forward * .009f, .0007f, Color.white);
            WorkLight(graph.Root, "Neutral work light", new Vector3(-.115f, .149f, -.052f), new Color(1f, .94f, .86f), 8.5f);
            WorkLight(graph.Root, "Cool security fill", new Vector3(.143f, .127f, .063f), new Color(.64f, .88f, 1f), 8.7f);
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

        private static string Row(SparkLevel115Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int offset, List<Vector3> occupied)
        {
            for (int ring = 0; ring < 12; ring++)
            for (int notch = 0; notch < 8; notch++)
            {
                float angle = (offset + notch) * Mathf.PI / 4f;
                float radius = .0054f + .0024f * ring;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0033f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .204f || Mathf.Abs(pad.z) > .143f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free SHX23-EDU-1 terminal at " + near);
        }
    }
}
