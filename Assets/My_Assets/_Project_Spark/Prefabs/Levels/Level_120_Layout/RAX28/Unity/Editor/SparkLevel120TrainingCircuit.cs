using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.AxisForge;
using UnityEngine;

namespace ProjectSpark.EditorTools.AxisForge
{
    internal static partial class SparkLevel120Builder
    {
        private static List<string> ConfigureGraph(SparkLevel120Process board, Circuit graph, string[][] plans,
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

            var routes = new List<SparkLevel120Process.Route>(SparkLevel120Process.RouteCount);
            var witnesses = new Dictionary<string, SparkTerminal>(StringComparer.Ordinal);
            var availableVias = new Queue<string>(viaIds);
            var pads = new List<Vector3>();
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var map = new List<string>
            {
                "task,domain,training_net,layer,from,to,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer"
            };
            var axisReturns = new Dictionary<string, SparkTerminal>();
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                var previous = board.input;
                int quota = witnesses.Count < 4 ? 9 : 8;
                int within = 0;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = group.Key switch
                    {
                        "24V_Entry" when within == 0 => "U14.TRAINING_INPUT_RETURN",
                        "Shared_DCLink" when within == 0 => "U11.SHARED_LINK_WITNESS",
                        "Dual_STO" when within < 2 => $"U{within + 4}.STO_RETURN",
                        "Encoders" when within < 6 => $"U{within + 22}.ENCODER_RETURN",
                        "Thermal" when within < 6 => within == 5 ? "U34.THERMAL_F_RETURN" : $"U{within + 16}.THERMAL_RETURN",
                        _ when within == 0 && group.Key.StartsWith("Axis_", StringComparison.Ordinal) =>
                            $"U{group.Key[5] - 'A' + 16}.MOTOR_STAGE_RETURN",
                        _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 37) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel120Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + group.Key,
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel120Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "copper" : "gold"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel120Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.00225f, .0002f, .00225f), materials["solder"]);
                        route.viaVisual.enabled = false;
                        if (role != null) axisReturns.Add(role, previous);
                        if (role == "U25.ENCODER_RETURN") board.encoderViaIndex = index;
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                    within++;
                }
                graph.Load(group.Key + " continuity return", previous, board.ground, 1800f);
                witnesses.Add(group.Key, previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel120Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel120Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel120Process.LayerCount ||
                board.parts.Any(part => !part.contact) || !axisReturns.ContainsKey("U25.ENCODER_RETURN"))
                throw new InvalidOperationException("The RAX28-EDU-1 topology is incomplete.");

            board.routes = routes.ToArray();
            board.witnesses = witnesses.Values.ToArray();
            (board.mainRail, board.bench) = Sense("BENCH_12V", board.input);
            (board.monitorSupply, board.monitorGate) = Sense("U48.INDEPENDENT_DIAGNOSTIC_12V", board.input);
            (board.prechargePin, board.prechargeGate) = Sense("U14.PRECHARGE_COMMAND", axisReturns["U14.TRAINING_INPUT_RETURN"]);
            (board.dcLinkPin, board.contactor) = Sense("U11.SHARED_DC_LINK_CONTACTOR", board.mainRail);
            (board.stoA, board.stoGateA) = Sense("U4.STO_A_VALID", axisReturns["U4.STO_RETURN"]);
            (board.stoB, board.stoGateB) = Sense("U5.STO_B_VALID", axisReturns["U5.STO_RETURN"]);
            board.gateWitness = witnesses["Gate_Drives"];
            board.resolverWitness = witnesses["ForceTorqueSensors"];
            board.currentWitness = witnesses["Shared_DCLink"];
            board.brakeWitness = witnesses["Regenerative_Brake"];
            board.drivePins = new SparkTerminal[6];
            board.encoderPins = new SparkTerminal[6];
            board.thermalPins = new SparkTerminal[6];
            board.driveGates = new SparkSwitch[6];
            board.encoderGates = new SparkSwitch[6];
            board.thermalGates = new SparkSwitch[6];
            board.axisMarkers = new Renderer[6];
            for (int axis = 0; axis < 6; axis++)
            {
                char letter = (char)('A' + axis);
                (board.drivePins[axis], board.driveGates[axis]) = Sense($"U{axis + 16}.AXIS_{letter}_DRIVE_ENABLE",
                    axisReturns[$"U{axis + 16}.MOTOR_STAGE_RETURN"]);
                (board.encoderPins[axis], board.encoderGates[axis]) = Sense($"U{axis + 22}.AXIS_{letter}_ENCODER_VALID",
                    axisReturns[$"U{axis + 22}.ENCODER_RETURN"]);
                string temperature = axis == 5 ? "U34.THERMAL_F_RETURN" : $"U{axis + 16}.THERMAL_RETURN";
                (board.thermalPins[axis], board.thermalGates[axis]) = Sense($"AXIS_{letter}.TEMPERATURE_DATA", axisReturns[temperature]);
                Vector3 site = Site("J" + (axis + 1));
                var marker = Marker(graph.Root, $"Axis {letter} drive witness", materials["status_blue"], .0054f);
                marker.transform.localPosition = new Vector3(.175f, .009f, site.z);
                board.axisMarkers[axis] = marker;
                BoardText(graph.Root, $"AXIS {letter}", new Vector3(.156f, .0017f, site.z - .013f), .00101f, Color.white);
            }
            (board.phaseB, board.phaseGate) = Sense("U18.AXIS_C_PHASE_B_HIGH", board.drivePins[2]);

            Vector3 a = FindPad(Site("U47"), 6, pads);
            Vector3 b = FindPad(Site("U48"), 2, pads);
            var sender = graph.Pin("U47.TRUSTED_SIX_AXIS_READY_OUT", a, true);
            var receiver = graph.Pin("U48.TRUSTED_SIX_AXIS_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U39 motion-ready heartbeat", board.monitorSupply, sender);
            var pullDown = graph.Load("U48 independent feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel120Process.Route
            {
                id = "W353", group = "Independent supervision", net = "TRUSTED_SIX_AXIS_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["gold"], "W353", a, b)
            };
            map.Add(Row(board.jumper));
            (board.protectedFeed, board.workload) = Sense("U48.PROTECTED_TORQUE_PERMIT", board.monitorSupply);
            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W353 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["gold"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["gold"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["phase_cyan"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["phase_cyan"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U48 torque permit indicator", PrimitiveType.Cube,
                b + new Vector3(.006f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["status_green"]);
            board.faultOverlays = new[] { Site("U18"), board.routes[board.encoderViaIndex].destination, Site("U34") }
                .Select((point, i) =>
                {
                    var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["alert_red"], .012f);
                    marker.transform.localPosition = point + Vector3.up * .013f;
                    marker.enabled = true;
                    marker.gameObject.SetActive(false);
                    return marker.gameObject;
                }).ToArray();
            BoardText(graph.Root, "RAX-28  /  THE AXIS FORGE", new Vector3(0, .0017f, -.176f), .00132f, Color.white);
            BoardText(graph.Root, "W353  /  U48", (a + b) * .5f + Vector3.right * .022f, .00091f, Color.white);
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

        private static string Row(SparkLevel120Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000f:F3},{route.entry.z * 1000f:F3},{route.destination.x * 1000f:F3},{route.destination.z * 1000f:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int start, List<Vector3> occupied)
        {
            for (int offset = 0; offset < 128; offset++)
            {
                int step = (start + offset) % 8;
                float angle = step * Mathf.PI * .25f;
                float radius = .0068f + (offset / 8) * .0025f;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .253f || Mathf.Abs(pad.z) > .183f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free RAX28-EDU-1 contact at " + near);
        }
    }
}
