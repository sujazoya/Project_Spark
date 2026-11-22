using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.ThermalAegis;
using UnityEngine;

namespace ProjectSpark.EditorTools.ThermalAegis
{
    internal static partial class SparkLevel127Builder
    {
        private static List<string> ConfigureGraph(SparkLevel127Process board, Circuit graph, string[][] plans,
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

            var routes = new List<SparkLevel127Process.Route>(SparkLevel127Process.RouteCount);
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
                int quota = witnesses.Count < 4 ? 24 : 23;
                int within = 0;
                foreach (var row in group)
                {
                    int index = routes.Count;
                    string role = (group.Key, within) switch
                    {
                        ("ProtectedPowerReturn", 0) => "M7.THERMAL_BENCH",
                        ("ProtectedPowerReturn", 1) => "U7.HEAT_PROFILE",
                        ("Redundancy", 0) => "U1.SUPERVISOR_A",
                        ("Redundancy", 1) => "U2.SUPERVISOR_B",
                        ("Redundancy", 2) => "R1.REFERENCE_B",
                        ("IndependentSafety", 0) => "U3.MONITOR_SUPPLY",
                        ("Telemetry", 0) => "U8.SAFE_TELEMETRY",
                        ("PumpDrive", 0) => "Q1.PUMP_A_RETURN",
                        ("PumpDrive", 1) => "M2.PUMP_B_RETURN",
                        ("HeaterControl", 0) => "M5.SURVIVAL_HEATER",
                        ("HeaterControl", 1) => "M6.BACKUP_HEATER",
                        ("RadiatorControl", 0) => "M3.RADIATOR_A",
                        ("RadiatorControl", 1) => "M4.RADIATOR_B",
                        ("ThermalSensor", < 12) => $"TH{within + 1}.SENSOR_RETURN",
                        _ => null
                    };
                    Vector3 site = role == null ? sites[(index * 59) % sites.Length] : Site(role.Split('.')[0]);
                    Vector3 start = FindPad(site, index % 8, pads);
                    Vector3 end = FindPad(site, (index + 3) % 8, pads);
                    var from = graph.Pin((role ?? row[0]) + ".A", start, true);
                    var to = graph.Pin((role ?? row[0]) + ".B", end, true);
                    var route = new SparkLevel127Process.Route
                    {
                        id = row[0], group = group.Key, net = role ?? "CHECK_" + group.Key,
                        from = from, to = to, entry = start, destination = end,
                        layer = index % SparkLevel127Process.LayerCount + 1,
                        copper = Copper(graph.Root, materials[role == null ? "copper" : "gold"], row[0], start, end)
                    };
                    var contact = graph.SwitchedOutput(row[0] + " assembly continuity", previous, from, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = to;
                    if (quota-- > 0)
                    {
                        route.viaId = availableVias.Dequeue();
                        route.exitLayer = route.layer % SparkLevel127Process.LayerCount + 1;
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
                graph.Load(group.Key + " continuity return", previous, board.ground, 1880f);
                witnesses.Add(group.Key, previous);
            }
            if (availableVias.Count != 0 || routes.Count != SparkLevel127Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel127Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel127Process.LayerCount ||
                board.parts.Any(part => !part.contact) || !sensorReturns.ContainsKey("TH1.SENSOR_RETURN"))
                throw new InvalidOperationException("The TCX35-EDU-1 topology is incomplete.");

            board.routes = routes.ToArray();
            board.witnesses = witnesses.Values.ToArray();
            (board.rail, board.bench) = Sense("M7.THERMAL_BENCH_RAIL", sensorReturns["M7.THERMAL_BENCH"]);
            (board.monitorSupply, board.monitorGate) = Sense("U3.INDEPENDENT_MONITOR", sensorReturns["U3.MONITOR_SUPPLY"]);
            (board.telemetryPin, board.telemetryGate) = Sense("U8.SAFE_TELEMETRY", sensorReturns["U8.SAFE_TELEMETRY"]);
            (board.heaterSupply, board.heaterPower) = Sense("M5.SURVIVAL_HEATER_RAIL", sensorReturns["M5.SURVIVAL_HEATER"]);
            (board.heaterBackup, board.heaterBackupGate) = Sense("M6.SURVIVAL_HEATER_BACKUP", sensorReturns["M6.BACKUP_HEATER"]);
            (board.heaterPin, board.heaterGate) = Sense("M5.SURVIVAL_HEAT_LOAD", board.heaterSupply);
            board.heaterLoad = graph.Load("M5 heater training shunt", board.heaterPin, board.ground, 560f);
            (board.referencePin, board.referenceGate) = Sense("R1.LOOP_B_REFERENCE", sensorReturns["R1.REFERENCE_B"]);
            board.referenceLoad = graph.Load("R1 independent temperature reference shunt", board.referencePin, board.ground, 100f);
            board.logicPins = new SparkTerminal[2];
            board.logicGates = new SparkSwitch[2];
            (board.logicPins[0], board.logicGates[0]) = Sense("U1.SUPERVISOR_A_LIVE", sensorReturns["U1.SUPERVISOR_A"]);
            (board.logicPins[1], board.logicGates[1]) = Sense("U2.SUPERVISOR_B_LIVE", sensorReturns["U2.SUPERVISOR_B"]);

            board.pumpPins = new SparkTerminal[2];
            board.pumpCommands = new SparkSwitch[2];
            board.pumpLoads = new SparkResistor[2];
            string[] pumpReturns = { "Q1.PUMP_A_RETURN", "M2.PUMP_B_RETURN" };
            for (int i = 0; i < pumpReturns.Length; i++)
            {
                var source = Sense(pumpReturns[i] + ".COMMAND", sensorReturns[pumpReturns[i]]);
                board.pumpCommands[i] = source.edge;
                board.pumpPins[i] = source.pin;
                if (i == 0)
                    (board.pumpPins[i], board.pumpAContact) = Sense("Q1.PUMP_A_DRIVER", source.pin, true);
                board.pumpLoads[i] = graph.Load("M" + (i + 1) + " pump feedback shunt",
                    board.pumpPins[i], board.ground, i == 0 ? 100f : 125f);
            }

            board.sensorPins = new SparkTerminal[12];
            board.sensorGates = new SparkSwitch[12];
            for (int i = 0; i < board.sensorPins.Length; i++)
            {
                string id = "TH" + (i + 1);
                var monitor = Sense(id + ".EXCITATION", sensorReturns[id + ".SENSOR_RETURN"]);
                board.sensorGates[i] = monitor.edge;
                board.sensorPins[i] = monitor.pin;
                if (i == 0)
                    (board.sensorPins[0], board.outletContact) = Sense("TH1.OUTLET_CONTACT", monitor.pin, true);
            }

            (board.baseHeatPin, board.baseHeatGate) = Sense("U7.CORE_DISSIPATION", sensorReturns["U7.HEAT_PROFILE"]);
            board.baseHeat = graph.Load("Core nominal heat training shunt", board.baseHeatPin, board.ground, 500f);
            (board.pulsePin, board.pulseGate) = Sense("U7.HEAT_PULSE", sensorReturns["U7.HEAT_PROFILE"]);
            board.pulseHeat = graph.Load("Core pulse training shunt", board.pulsePin, board.ground, 500f);

            var radiatorA = Sense("M3.RADIATOR_A_SUPPLY", sensorReturns["M3.RADIATOR_A"]);
            var radiatorB = Sense("M4.RADIATOR_B_SUPPLY", sensorReturns["M4.RADIATOR_B"]);
            board.radiatorSupply = radiatorA.pin;
            board.radiatorBackup = radiatorB.pin;
            board.radiatorPower = radiatorA.edge;
            board.radiatorBackupPower = radiatorB.edge;
            (board.testRadiatorPin, board.testRadiatorGate) = Sense("M3.RADIATOR_DUMMY_LOAD", radiatorA.pin);
            board.testRadiatorLoad = graph.Load("Radiator training feedback", board.testRadiatorPin, board.ground, 220f);
            (board.protectedFeed, board.workload) = Sense("U3.PROTECTED_RADIATOR_PERMIT", radiatorA.pin);
            board.radiatorPins = new SparkTerminal[2];
            board.radiatorLoads = new SparkResistor[2];
            for (int i = 0; i < board.radiatorPins.Length; i++)
            {
                var actuator = Sense($"M{i + 3}.PROTECTED_RADIATOR", board.protectedFeed, true);
                board.radiatorPins[i] = actuator.pin;
                board.radiatorLoads[i] = graph.Load($"M{i + 3} radiator feedback shunt", actuator.pin, board.ground, 440f);
            }

            board.sensorMarkers = new Renderer[4];
            string[] labels = { "PUMP A", "PUMP B", "SURVIVAL", "RADIATORS" };
            string[] sitesForLabels = { "M1", "M2", "M5", "M3" };
            for (int i = 0; i < labels.Length; i++)
            {
                Vector3 site = Site(sitesForLabels[i]);
                var marker = Marker(graph.Root, labels[i] + " status", materials["blue"], .0042f);
                marker.transform.localPosition = site + new Vector3(-.018f, .018f, 0f);
                board.sensorMarkers[i] = marker;
                BoardText(graph.Root, labels[i], site + new Vector3(0f, .0029f, -.024f), .00115f, Color.white);
            }

            Vector3 a = FindPad(Site("U6"), 6, pads);
            Vector3 b = FindPad(Site("U3"), 2, pads);
            var sender = graph.Pin("U6.TRUSTED_THERMAL_READY_OUT", a, true);
            var receiver = graph.Pin("U3.TRUSTED_THERMAL_READY_IN", b, true);
            board.heartbeat = graph.SwitchedOutput("U6 thermal-ready heartbeat", board.monitorSupply, sender);
            var pullDown = graph.Load("U3 independent feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(pullDown.transform, true);
            board.jumper = new SparkLevel127Process.Route
            {
                id = "W465", group = "Independent supervision", net = "TRUSTED_THERMAL_READY", layer = 1,
                from = sender, to = receiver, entry = a, destination = b,
                copper = Copper(graph.Root, materials["gold"], "W465", a, b)
            };
            map.Add(Row(board.jumper));
            foreach (Vector3 point in new[] { a, b })
                Primitive(graph.Root, "W465 training contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0034f, .00022f, .0034f), materials["gold"]);
            board.startMarker = Marker(graph.Root, "Copper source", materials["gold"], .0036f);
            board.endMarker = Marker(graph.Root, "Copper destination", materials["blue"], .0036f);
            board.locationMarker = Marker(graph.Root, "Placement locator", materials["blue"], .0056f);
            board.statusLamp = Primitive(graph.Root, "U3 radiator permit indicator", PrimitiveType.Cube,
                b + new Vector3(.006f, .0006f, .007f), new Vector3(.0031f, .0006f, .0022f), materials["green"]);
            board.faultOverlays = new[] { Site("Q1"), Site("R1"), Site("TH1") }
                .Select((point, i) =>
                {
                    var marker = Marker(graph.Root, $"Fault probe {i + 1}", materials["red"], .012f);
                    marker.transform.localPosition = point + Vector3.up * .013f;
                    marker.enabled = true;
                    marker.gameObject.SetActive(false);
                    return marker.gameObject;
                }).ToArray();
            BoardText(graph.Root, "TCX-35  /  THE THERMAL AEGIS", new Vector3(0, .0017f, -.251f), .00132f, Color.white);
            BoardText(graph.Root, "W465  /  U3", (a + b) * .5f + Vector3.right * .022f, .00091f, Color.white);
            WorkLight(graph.Root, "Neutral task light", new Vector3(-.127f, .149f, -.062f), new Color(1f, .94f, .86f), 4.9f);
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

        private static string Row(SparkLevel127Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000f:F3},{route.entry.z * 1000f:F3},{route.destination.x * 1000f:F3},{route.destination.z * 1000f:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 near, int start, List<Vector3> occupied)
        {
            for (int offset = 0; offset < 128; offset++)
            {
                int step = (start + offset) % 8;
                float angle = step * Mathf.PI * .25f;
                float radius = .0068f + (offset / 8) * .0025f;
                var pad = new Vector3(near.x + Mathf.Cos(angle) * radius, .0034f, near.z + Mathf.Sin(angle) * radius);
                if (Mathf.Abs(pad.x) > .323f || Mathf.Abs(pad.z) > .253f ||
                    occupied.Any(other => (other - pad).sqrMagnitude < .0000042f)) continue;
                occupied.Add(pad);
                return pad;
            }
            throw new InvalidOperationException("No free TCX35-EDU-1 contact at " + near);
        }
    }
}
