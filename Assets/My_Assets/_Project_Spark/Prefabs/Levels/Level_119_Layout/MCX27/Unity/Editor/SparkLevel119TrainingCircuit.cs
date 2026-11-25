using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.MotionCitadel;
using UnityEngine;

namespace ProjectSpark.EditorTools.MotionCitadel
{
    internal static partial class SparkLevel119Builder
    {
        private static List<string> ConfigureGraph(SparkLevel119Process board, Circuit graph, string[][] plans,
            IEnumerable<string> viaIds, Dictionary<string, Material> materials)
        {
            Vector3 Site(string reference) =>
                graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == reference).footprint.position);

            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 12f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity test return", board.input, board.ground, 10000f);

            var routes = new List<SparkLevel119Process.Route>(SparkLevel119Process.RouteCount);
            var witnesses = new Dictionary<string, SparkTerminal>(StringComparer.Ordinal);
            var availableVias = new Queue<string>(viaIds);
            var pads = new List<Vector3>();
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var map = new List<string>
            {
                "task,domain,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer"
            };
            SparkTerminal encoderVia = null;
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                var previous = board.input;
                int quota = witnesses.Count < 4 ? 9 : 8;
                int within = 0;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = within == 0 ? group.Key switch
                    {
                        "DC_Link_Ground" => "U18.LINK_RETURN_WITNESS",
                        "Precharge_Power" => "U15.PRECHARGE_WITNESS",
                        "Gate_Drives" => "U12.ISOLATED_GATE_WITNESS",
                        "Phase_U" => "Q1.U_LEG_WITNESS",
                        "Phase_V" => "Q2.V_LEG_WITNESS",
                        "Phase_W" => "Q3.W_LEG_WITNESS",
                        "Encoder_Resolver" => "U9.ENCODER_ESCAPE",
                        "Brake_Chopper" => "U14.BRAKE_WITNESS",
                        "STO_A" => "U4.STO_A_WITNESS",
                        "STO_B" => "U5.STO_B_WITNESS",
                        _ => null
                    } : null;
                    Vector3 site = role == null ? sites[(index * 37) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel119Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + group.Key,
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel119Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "copper" : "gold"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel119Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.00225f, .0002f, .00225f), materials["solder"]);
                        route.viaVisual.enabled = false;
                        if (within == 0 && group.Key == "Encoder_Resolver")
                        {
                            board.encoderViaIndex = index;
                            encoderVia = previous;
                        }
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                    within++;
                }
                graph.Load(group.Key + " continuity return", previous, board.ground, 1800f);
                witnesses.Add(group.Key, previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel119Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel119Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel119Process.LayerCount ||
                board.parts.Any(part => !part.contact) || !encoderVia)
                throw new InvalidOperationException("The MCX27-EDU-1 topology is incomplete.");

            board.routes = routes.ToArray();
            board.witnesses = witnesses.Values.ToArray();
            (board.mainRail, board.bench) = Sense("BENCH_12V", board.input);
            (board.monitorSupply, board.monitorGate) = Sense("U40.INDEPENDENT_DIAGNOSTIC_12V", board.input);
            (board.prechargePin, board.prechargeGate) = Sense("U15.PRECHARGE_COMMAND", witnesses["Precharge_Power"]);
            (board.dcLinkPin, board.contactor) = Sense("U18.DC_LINK_CONTACTOR", board.mainRail);
            (board.stoA, board.stoGateA) = Sense("U4.STO_A_VALID", witnesses["STO_A"]);
            (board.stoB, board.stoGateB) = Sense("U5.STO_B_VALID", witnesses["STO_B"]);
            (board.phaseU, board.gateU) = Sense("Q1.PHASE_U_ENABLED", witnesses["Phase_U"]);
            (board.phaseV, board.gateV) = Sense("U12.Q2.PHASE_V_HIGH", witnesses["Phase_V"]);
            (board.phaseW, board.gateW) = Sense("Q3.PHASE_W_ENABLED", witnesses["Phase_W"]);
            (board.encoderPin, board.encoderGate) = Sense("U9.PRIMARY_ENCODER_ESCAPE", encoderVia);
            (board.brakePin, board.brakeGate) = Sense("U14.BRAKE_SENSOR_VALID", witnesses["Brake_Chopper"]);
            board.gateWitness = witnesses["Gate_Drives"];
            board.currentWitness = witnesses["Current_Sensing"];
            board.resolverWitness = witnesses["Encoder_Resolver"];

            Vector3 a = FindPad(Site("U39"), 6, pads);
            Vector3 b = FindPad(Site("U40"), 2, pads);
            var sender = graph.Pin("U39.TRUSTED_MOTION_READY_OUT", a, true);
            var receiver = graph.Pin("U40.TRUSTED_MOTION_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U39 motion-ready heartbeat", board.monitorSupply, sender);
            var pullDown = graph.Load("U40 independent feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel119Process.Route
            {
                id = "W337", group = "Independent supervision", net = "TRUSTED_MOTION_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["gold"], "W337", a, b)
            };
            map.Add(Row(board.jumper));
            (board.protectedFeed, board.workload) = Sense("U40.PROTECTED_TORQUE_PERMIT", board.monitorSupply);
            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W337 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["gold"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["gold"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["phase_cyan"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["phase_cyan"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U40 torque permit indicator", PrimitiveType.Cube,
                b + new Vector3(.006f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["status_green"]);
            board.faultOverlays = new[] { Site("U12"), board.routes[board.encoderViaIndex].destination, Site("U14") }
                .Select((point, i) =>
                {
                    var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["alert_red"], .012f);
                    marker.transform.localPosition = point + Vector3.up * .013f;
                    marker.enabled = true;
                    marker.gameObject.SetActive(false);
                    return marker.gameObject;
                }).ToArray();
            BoardText(graph.Root, "MCX-27  /  THE MOTION CITADEL", new Vector3(0, .0017f, -.168f), .00132f, Color.white);
            BoardText(graph.Root, "W337  /  U40", (a + b) * .5f + Vector3.right * .022f, .00091f, Color.white);
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

        private static string Row(SparkLevel119Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000f:F3},{route.entry.z * 1000f:F3},{route.destination.x * 1000f:F3},{route.destination.z * 1000f:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int start, List<Vector3> occupied)
        {
            for (int offset = 0; offset < 128; offset++)
            {
                int step = (start + offset) % 8;
                float angle = step * Mathf.PI * .25f;
                float radius = .0068f + (offset / 8) * .0025f;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .244f || Mathf.Abs(pad.z) > .173f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free MCX27-EDU-1 contact at " + near);
        }
    }
}
