using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.OrbitalBastion;
using UnityEngine;

namespace ProjectSpark.EditorTools.OrbitalBastion
{
    internal static partial class SparkLevel124Builder
    {
        private static List<string> ConfigureGraph(SparkLevel124Process board, Circuit graph, string[][] plans,
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

            var routes = new List<SparkLevel124Process.Route>(SparkLevel124Process.RouteCount);
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
                int quota = witnesses.Count < 2 ? 28 : 27;
                int within = 0;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = (group.Key, within) switch
                    {
                        ("ProtectedPowerReturn", 0) => "U13.COMPUTE_POWER",
                        ("ProtectedPowerReturn", 1) => "U14.WHEEL_LOAD_POWER",
                        ("StarTrackerClock", 0) => "Y1.CLOCK_A",
                        ("StarTrackerClock", 1) => "Y2.CLOCK_B",
                        ("StarTrackerClock", 2) => "U7.TRACKER_B_REF",
                        ("StarTrackerClock", 3) => "U6.TRACKER_A_REF",
                        ("SafetyAndFDIR", 0) => "U3.RESET_RELEASE",
                        ("RedundantTelemetry", 0) => "U1.COMPUTE_A",
                        ("RedundantTelemetry", 1) => "U2.COMPUTE_B",
                        ("RedundantTelemetry", 2) => "U5.SAFE_TELEMETRY",
                        ("ReactionWheelDrive", < 4) => $"J{within + 1}.WHEEL_RETURN",
                        ("AttitudeSensorSync", 0) => "SN1.GYRO",
                        ("AttitudeSensorSync", 1) => "U8.SUN_REFERENCE",
                        ("AttitudeSensorSync", 2) => "SN4.WHEEL_TACH",
                        _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 59) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel124Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + group.Key,
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel124Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "copper" : "gold"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel124Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.00225f, .0002f, .00225f), materials["solder"]);
                        route.viaVisual.enabled = false;
                        if (role != null) sensorReturns.Add(role, previous);
                        if (role == "U7.TRACKER_B_REF") board.trackerViaIndex = index;
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                    within++;
                }
                graph.Load(group.Key + " continuity return", previous, board.ground, 1800f);
                witnesses.Add(group.Key, previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel124Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel124Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel124Process.LayerCount ||
                board.parts.Any(part => !part.contact) || !sensorReturns.ContainsKey("U7.TRACKER_B_REF"))
                throw new InvalidOperationException("The OBX32-EDU-1 topology is incomplete.");

            board.routes = routes.ToArray();
            board.witnesses = witnesses.Values.ToArray();
            (board.rail, board.bench) = Sense("U13.CONTROLLER_RAIL", sensorReturns["U13.COMPUTE_POWER"]);
            (board.driveRail, board.drivePower) = Sense("U14.WHEEL_LOAD_RAIL", sensorReturns["U14.WHEEL_LOAD_POWER"]);
            (board.monitorSupply, board.monitorGate) = Sense("U3.INDEPENDENT_DIAGNOSTIC_5V", board.input);
            (board.telemetryPin, board.telemetryGate) = Sense("U5.SAFE_MODE_TELEMETRY", sensorReturns["U5.SAFE_TELEMETRY"]);
            (board.clockPin, board.clockGate) = Sense("Y1.TIMEBASE_A_LIVE", sensorReturns["Y1.CLOCK_A"]);
            (board.secondaryClock, board.secondaryClockGate) = Sense("Y2.TIMEBASE_B_LIVE", sensorReturns["Y2.CLOCK_B"]);
            (board.resetPin, board.resetGate) = Sense("U3.RESET_RELEASE_LIVE", sensorReturns["U3.RESET_RELEASE"]);
            board.logicPins = new SparkTerminal[2];
            board.logicGates = new SparkSwitch[2];
            (board.logicPins[0], board.logicGates[0]) = Sense("U1.COMPUTE_A_LIVE", sensorReturns["U1.COMPUTE_A"]);
            (board.logicPins[1], board.logicGates[1]) = Sense("U2.COMPUTE_B_LIVE", sensorReturns["U2.COMPUTE_B"]);

            board.wheelPins = new SparkTerminal[4];
            board.wheelPower = new SparkSwitch[4];
            board.gatePins = new SparkTerminal[4];
            board.outputContacts = new SparkSwitch[4];
            board.wheelLoads = new SparkResistor[4];
            board.sensorMarkers = new Renderer[4];
            for (int i = 0; i < board.wheelPins.Length; i++)
            {
                string siteId = "J" + (i + 1);
                string trace = siteId + ".WHEEL_RETURN";
                (board.wheelPins[i], board.wheelPower[i]) = Sense(trace + ".BENCH", sensorReturns[trace]);
                board.wheelLoads[i] = graph.Load(siteId + " wheel load shunt", board.wheelPins[i], board.ground, 100f);
                Vector3 site = Site(siteId);
                var marker = Marker(graph.Root, "Wheel " + (i + 1) + " status", materials["blue"], .0042f);
                marker.transform.localPosition = site + new Vector3(-.021f, .011f, 0f);
                board.sensorMarkers[i] = marker;
                BoardText(graph.Root, $"RW{i + 1}", site + new Vector3(-.031f, .002f, -.013f), .00115f, Color.white);
            }

            board.sensorPins = new SparkTerminal[5];
            board.sensorGates = new SparkSwitch[5];
            string[] roles = { "U6.TRACKER_A_REF", "U7.TRACKER_B_REF", "SN1.GYRO", "U8.SUN_REFERENCE", "SN4.WHEEL_TACH" };
            string[] labels = { "STAR A", "STAR B", "GYRO", "SUN", "TACH" };
            for (int i = 0; i < roles.Length; i++)
            {
                (board.sensorPins[i], board.sensorGates[i]) = Sense(roles[i] + ".LIVE", sensorReturns[roles[i]]);
                if (i == 1)
                    (board.sensorPins[i], board.trackerContact) = Sense("U7.TRACKER_B_VIA_CONTACT", board.sensorPins[i], true);
                Vector3 site = Site(roles[i].Split('.')[0]);
                BoardText(graph.Root, labels[i], site + new Vector3(0, .002f, -.024f), .001f, Color.white);
            }
            board.tachReference = graph.Pin("SN4.TACH_BIAS_REFERENCE");
            board.tachContact = graph.SwitchedOutput("SN4 tachometer reference contact", board.sensorPins[4], board.tachReference, true);
            graph.Load("Tachometer reference bias", board.sensorPins[4], board.tachReference, 330f);
            graph.Load("Tachometer reference return", board.tachReference, board.ground, 1000f);

            Vector3 a = FindPad(Site("U5"), 6, pads);
            Vector3 b = FindPad(Site("U3"), 2, pads);
            var sender = graph.Pin("U5.TRUSTED_ATTITUDE_READY_OUT", a, true);
            var receiver = graph.Pin("U3.TRUSTED_ATTITUDE_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U5 attitude-ready heartbeat", board.monitorSupply, sender);
            var pullDown = graph.Load("U3 independent feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel124Process.Route
            {
                id = "W417", group = "Independent supervision", net = "TRUSTED_ATTITUDE_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["gold"], "W417", a, b)
            };
            map.Add(Row(board.jumper));
            (board.protectedFeed, board.workload) = Sense("U3.PROTECTED_ATTITUDE_PERMIT", board.driveRail);
            for (int i = 0; i < board.gatePins.Length; i++)
                (board.gatePins[i], board.outputContacts[i]) = Sense($"J{i + 1}.PROTECTED_GATE", board.protectedFeed);
            (board.unloadPin, board.unloadGate) = Sense("U10.DESATURATION_GATE", board.protectedFeed);
            BoardText(graph.Root, "MOMENTUM DUMP", Site("U10") + new Vector3(.003f, .002f, -.027f), .00102f, Color.white);
            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W417 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["gold"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["gold"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["blue"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["blue"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U3 attitude permit indicator", PrimitiveType.Cube,
                b + new Vector3(.006f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["green"]);
            board.faultOverlays = new[] { Site("J3"), board.routes[board.trackerViaIndex].destination, Site("SN4") }
                .Select((point, i) =>
                {
                    var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["red"], .012f);
                    marker.transform.localPosition = point + Vector3.up * .013f;
                    marker.enabled = true;
                    marker.gameObject.SetActive(false);
                    return marker.gameObject;
                }).ToArray();
            BoardText(graph.Root, "OBX-32  /  THE ORBITAL BASTION", new Vector3(0, .0017f, -.221f), .00132f, Color.white);
            BoardText(graph.Root, "W417  /  U3", (a + b) * .5f + Vector3.right * .022f, .00091f, Color.white);
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

        private static string Row(SparkLevel124Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000f:F3},{route.entry.z * 1000f:F3},{route.destination.x * 1000f:F3},{route.destination.z * 1000f:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int start, List<Vector3> occupied)
        {
            for (int offset = 0; offset < 128; offset++)
            {
                int step = (start + offset) % 8;
                float angle = step * Mathf.PI * .25f;
                float radius = .0068f + (offset / 8) * .0025f;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .293f || Mathf.Abs(pad.z) > .223f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free OBX32-EDU-1 contact at " + near);
        }
    }
}
