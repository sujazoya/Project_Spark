using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.EclipseCore;
using UnityEngine;

namespace ProjectSpark.EditorTools.EclipseCore
{
    internal static partial class SparkLevel126Builder
    {
        private static List<string> ConfigureGraph(SparkLevel126Process board, Circuit graph, string[][] plans,
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

            var routes = new List<SparkLevel126Process.Route>(SparkLevel126Process.RouteCount);
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
                int quota = witnesses.Count < 4 ? 23 : 22;
                int within = 0;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = (group.Key, within) switch
                    {
                        ("ProtectedPowerReturn", 0) => "M5.PROTECTED_BUS_A",
                        ("ProtectedPowerReturn", 1) => "M6.PROTECTED_BUS_B",
                        ("ProtectedPowerReturn", 2) => "U1.ENERGY_SUPERVISOR_A",
                        ("ProtectedPowerReturn", 3) => "U2.ENERGY_SUPERVISOR_B",
                        ("IndependentInhibit", 0) => "U7.THERMAL_TELEMETRY",
                        ("IndependentInhibit", 1) => "U8.SAFE_TELEMETRY",
                        ("SolarA", 0) => "Q1.SOLAR_A_RETURN",
                        ("SolarB", 0) => "M2.SOLAR_B_RETURN",
                        ("BatteryA", 0) => "M3.CHARGE_RETURN",
                        ("BatteryB", 0) => "M4.DISCHARGE_RETURN",
                        ("BatteryB", 1) => "R1.SENSE_REFERENCE",
                        ("EssentialBus", 0) => "M7.ESSENTIAL_RETURN",
                        ("EssentialBus", 1) => "C1.LOAD_STEP_RETURN",
                        ("AuxiliaryBus", 0) => "M8.AUXILIARY_RETURN",
                        _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 59) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel126Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + group.Key,
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel126Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "copper" : "gold"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel126Process.LayerCount + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", to, previous);
                        route.viaVisual = Primitive(graph.Root, route.viaId + " plated land", PrimitiveType.Cylinder,
                            end, new Vector3(.00225f, .0002f, .00225f), materials["solder"]);
                        route.viaVisual.enabled = false;
                        if (role != null) sensorReturns.Add(role, previous);
                    }
                    routes.Add(route);
                    map.Add(Row(route));
                    within++;
                }
                graph.Load(group.Key + " continuity return", previous, board.ground, 1800f);
                witnesses.Add(group.Key, previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel126Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel126Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel126Process.LayerCount ||
                board.parts.Any(part => !part.contact) || !sensorReturns.ContainsKey("C1.LOAD_STEP_RETURN"))
                throw new InvalidOperationException("The EPX34-EDU-1 topology is incomplete.");

            board.routes = routes.ToArray();
            board.witnesses = witnesses.Values.ToArray();
            (board.rail, board.bench) = Sense("M5.PROTECTED_BUS_A_LIVE", sensorReturns["M5.PROTECTED_BUS_A"]);
            (board.secondaryRail, board.secondaryPower) = Sense("M6.PROTECTED_BUS_B_LIVE", sensorReturns["M6.PROTECTED_BUS_B"]);
            (board.monitorSupply, board.monitorGate) = Sense("U3.INDEPENDENT_DIAGNOSTIC_5V", board.input);
            (board.telemetryPin, board.telemetryGate) = Sense("U8.SAFE_TELEMETRY_LIVE", sensorReturns["U8.SAFE_TELEMETRY"]);
            (board.thermalPin, board.thermalGate) = Sense("U7.THERMAL_TELEMETRY_LIVE", sensorReturns["U7.THERMAL_TELEMETRY"]);
            board.logicPins = new SparkTerminal[2];
            board.logicGates = new SparkSwitch[2];
            (board.logicPins[0], board.logicGates[0]) = Sense("U1.ENERGY_SUPERVISOR_A_LIVE", sensorReturns["U1.ENERGY_SUPERVISOR_A"]);
            (board.logicPins[1], board.logicGates[1]) = Sense("U2.ENERGY_SUPERVISOR_B_LIVE", sensorReturns["U2.ENERGY_SUPERVISOR_B"]);
            (board.chargePin, board.chargeGate) = Sense("M3.CHARGE_LIVE", sensorReturns["M3.CHARGE_RETURN"]);
            (board.dischargePin, board.dischargeGate) = Sense("M4.DISCHARGE_LIVE", sensorReturns["M4.DISCHARGE_RETURN"]);
            (board.sensePin, board.senseGate) = Sense("R1.SIGNED_CURRENT_CALIBRATION", sensorReturns["R1.SENSE_REFERENCE"]);
            board.batterySense = graph.Load("R1 battery sense calibration shunt", board.sensePin, board.ground, 100f);

            board.solarPins = new SparkTerminal[2];
            board.solarGates = new SparkSwitch[2];
            board.solarLoads = new SparkResistor[2];
            string[] returns = { "Q1.SOLAR_A_RETURN", "M2.SOLAR_B_RETURN" };
            for (int i = 0; i < returns.Length; i++)
            {
                (board.solarPins[i], board.solarGates[i]) = Sense(returns[i] + ".BENCH", sensorReturns[returns[i]]);
                board.solarLoads[i] = graph.Load(returns[i] + " training generation shunt", board.solarPins[i], board.ground, 250f);
            }

            var essential = Sense("M7.ESSENTIAL_BUS_TEST_SUPPLY", sensorReturns["M7.ESSENTIAL_RETURN"]);
            board.essentialGate = essential.edge;
            board.essentialPin = graph.Pin("C1.ESSENTIAL_FILTER_OUTPUT");
            board.filterSeries = graph.Load("C1 equivalent series loss", essential.pin, board.essentialPin, .12f);
            board.essentialPin.transform.SetParent(board.filterSeries.transform, true);
            board.essentialLoad = graph.Load("M7 essential load shunt", board.essentialPin, board.ground, 375f);
            var pulse = Sense("C1.LOAD_STEP", board.essentialPin);
            board.stepGate = pulse.edge;
            graph.Load("Essential bus short diagnostic load step", pulse.pin, board.ground, 750f);
            var auxFeed = Sense("M8.AUXILIARY_BUS_SUPPLY", sensorReturns["M8.AUXILIARY_RETURN"]);
            board.auxPower = auxFeed.edge;
            (board.testAuxPin, board.testAuxGate) = Sense("M8.ISOLATED_DUMMY_LOAD", auxFeed.pin);
            board.testAuxLoad = graph.Load("M8 auxiliary diagnostic load shunt", board.testAuxPin, board.ground, 500f);
            (board.protectedFeed, board.workload) = Sense("U3.PROTECTED_AUX_PERMIT", auxFeed.pin);
            board.protectedLoad = graph.Load("M8 protected auxiliary load shunt", board.protectedFeed, board.ground, 500f);

            board.sensorMarkers = new Renderer[4];
            string[] labels = { "SOLAR A", "SOLAR B", "ESSENTIAL", "AUXILIARY" };
            string[] sitesForLabels = { "J1", "J2", "J5", "J6" };
            for (int i = 0; i < labels.Length; i++)
            {
                Vector3 site = Site(sitesForLabels[i]);
                var marker = Marker(graph.Root, labels[i] + " status", materials["blue"], .0042f);
                marker.transform.localPosition = site + new Vector3(-.021f, .016f, 0f);
                board.sensorMarkers[i] = marker;
                BoardText(graph.Root, labels[i], site + new Vector3(-.026f, .0029f, -.018f), .00115f, Color.white);
            }

            Vector3 a = FindPad(Site("U6"), 6, pads);
            Vector3 b = FindPad(Site("U3"), 2, pads);
            var sender = graph.Pin("U6.TRUSTED_ENERGY_READY_OUT", a, true);
            var receiver = graph.Pin("U3.TRUSTED_ENERGY_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U6 energy-ready heartbeat", board.monitorSupply, sender);
            var pullDown = graph.Load("U3 independent feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel126Process.Route
            {
                id = "W449", group = "Independent supervision", net = "TRUSTED_ENERGY_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["gold"], "W449", a, b)
            };
            map.Add(Row(board.jumper));
            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W449 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["gold"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["gold"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["blue"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["blue"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U3 auxiliary permit indicator", PrimitiveType.Cube,
                b + new Vector3(.006f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["green"]);
            board.faultOverlays = new[] { Site("Q1"), Site("R1"), Site("C1") }
                .Select((point, i) =>
                {
                    var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["red"], .012f);
                    marker.transform.localPosition = point + Vector3.up * .013f;
                    marker.enabled = true;
                    marker.gameObject.SetActive(false);
                    return marker.gameObject;
                }).ToArray();
            BoardText(graph.Root, "EPX-34  /  THE ECLIPSE CORE", new Vector3(0, .0017f, -.241f), .00132f, Color.white);
            BoardText(graph.Root, "W449  /  U3", (a + b) * .5f + Vector3.right * .022f, .00091f, Color.white);
            WorkLight(graph.Root, "Neutral task light", new Vector3(-.126f, .149f, -.062f), new Color(1f, .94f, .86f), 4.9f);
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

        private static string Row(SparkLevel126Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000f:F3},{route.entry.z * 1000f:F3},{route.destination.x * 1000f:F3},{route.destination.z * 1000f:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int start, List<Vector3> occupied)
        {
            for (int offset = 0; offset < 128; offset++)
            {
                int step = (start + offset) % 8;
                float angle = step * Mathf.PI * .25f;
                float radius = .0068f + (offset / 8) * .0025f;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .313f || Mathf.Abs(pad.z) > .243f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free EPX34-EDU-1 contact at " + near);
        }
    }
}
