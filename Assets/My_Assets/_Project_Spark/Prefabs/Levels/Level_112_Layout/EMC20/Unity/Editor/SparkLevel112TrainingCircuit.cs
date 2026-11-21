using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.EmcGauntlet;
using UnityEngine;

namespace ProjectSpark.EditorTools.EmcGauntlet
{
    internal static partial class SparkLevel112Builder
    {
        private const string StitchTask = "W139";
        private const string StitchVia = "V44";

        private static List<string> ConfigureGraph(SparkLevel112Process board, Circuit graph, string[][] plans,
            IEnumerable<string> viaIds, Dictionary<string, Material> mats)
        {
            Vector3 Site(string id) => graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == id).footprint.position);
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 5f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity reference", board.input, board.ground, 10000f);
            board.standby = graph.Pin("EMC20.PROTECTED_5V");
            board.bench = graph.SwitchedOutput("Bench isolation", board.input, board.standby);
            board.stimulus = graph.Load("Switching workload current witness", board.standby, board.ground, 20f);
            var raw = graph.Pin("EMC20.AUX_SOURCE");
            graph.Supply("3.3 V training regulator", raw, board.ground, 3.3f);
            board.aux = graph.Pin("EMC20.AUX_3V3");
            board.auxGate = graph.SwitchedOutput("Auxiliary rail", raw, board.aux);
            graph.Load("Auxiliary witness", board.aux, board.ground, 1000f);
            var fixtureDrive = graph.Pin("U10.CAL_REFERENCE");
            board.fixtureGate = graph.SwitchedOutput("U10 fixture calibration pulse", board.aux, fixtureDrive);
            board.fixtureSense = graph.Pin("U2.FIXTURE_REFERENCE");
            var divider = graph.Load("Fixture reference feed", fixtureDrive, board.fixtureSense, 1000f);
            board.fixtureSense.transform.SetParent(divider.transform, true);
            graph.Load("Reference fixture termination", board.fixtureSense, board.ground, 1000f);
            board.ditherSense = graph.Pin("U6.SPREAD_ENABLE");
            board.ditherGate = graph.SwitchedOutput("U6 spread-spectrum enable", board.aux, board.ditherSense);
            graph.Load("Spread enable pull-down", board.ditherSense, board.ground, 10000f);
            board.returnSense = graph.Pin("U12.STITCH_REFERENCE");

            board.lanes = Enumerable.Range(0, 4).Select(_ => new SparkLevel112Process.Lane()).ToArray();
            var pads = new List<Vector3>();
            var routes = new List<SparkLevel112Process.Route>(SparkLevel112Process.RouteCount);
            var remaining = new Queue<string>(viaIds.Where(id => id != StitchVia));
            var witnesses = new List<SparkTerminal>();
            var sites = board.parts.Select(part => Site(part.label)).ToArray();
            var shieldSite = graph.Root.InverseTransformPoint(board.shields[0].transform.position);
            var shieldEar = shieldSite + new Vector3(.032f, 0, -.014f);
            var csv = new List<string> { "task,domain,training_net,entry_layer,source_pad,destination_pad,from_x_mm,from_z_mm,to_x_mm,to_z_mm,via,exit_layer" };
            foreach (var group in plans.GroupBy(row => row[1]))
            {
                int quota = witnesses.Count < 5 ? 10 : 9;
                if (group.Key == "Return path and shield bonding") quota--;
                var previous = board.input;
                foreach (var row in group)
                {
                    int i = routes.Count;
                    bool clock = i is 30 or 31, choke = i == 90, shield = i == 120, stitch = row[0] == StitchTask;
                    int dataLeg = i is >= 60 and < 68 ? i - 60 : -1;
                    bool functional = clock || choke || shield || stitch || dataLeg >= 0;
                    string net = clock ? i == 30 ? "REFCLK_P" : "REFCLK_N" : choke ? "FILTER_BIAS" :
                        shield ? "CAN1_BOND" : stitch ? "RETURN_STITCH" : dataLeg >= 0 ?
                        $"LANE{dataLeg / 2}_{(dataLeg % 2 == 0 ? "P" : "N")}" : $"CHECK_{witnesses.Count + 1}";
                    string source = clock ? "U5" : choke ? "L1" : shield ? "U20" : stitch ? "U12" : "U7";
                    string sink = clock ? "U2" : choke ? "U18" : stitch ? "U12" : "U13";
                    Vector3 startSite = functional ? Site(source) : sites[i % sites.Length];
                    Vector3 endSite = shield ? shieldEar : functional ? Site(sink) : startSite;
                    if (stitch) endSite += new Vector3(.014f, 0f, .019f);
                    var start = FindPad(startSite, i % 8, pads);
                    var end = FindPad(endSite, (i + 2) % 8, pads);
                    var a = graph.Pin(functional ? source + "." + net + ".A" : row[0] + ".A", start, true);
                    var b = graph.Pin(functional ? (shield ? "CAN1" : sink) + "." + net + ".B" : row[0] + ".B", end, true);
                    var route = new SparkLevel112Process.Route
                    {
                        id = row[0], group = group.Key, net = net, from = a, to = b, entry = start, destination = end,
                        layer = stitch ? 6 : shield ? 2 : clock ? 5 : choke ? 4 : dataLeg >= 0 ? 7 : i % SparkLevel112Process.LayerCount + 1,
                        copper = Copper(graph.Root, mats[functional ? "Gold_ENIG" : "Copper_Exposed"], row[0], start, end)
                    };
                    var checkedPin = functional ? graph.Pin(row[0] + ".ASSEMBLY_CHECK") : a;
                    var solder = graph.SwitchedOutput(row[0] + " solder witness", previous, checkedPin, i >= board.parts.Length);
                    if (i < board.parts.Length) board.parts[i].contact = solder;
                    previous = functional ? checkedPin : b;
                    if (clock)
                    {
                        var drive = graph.SwitchedOutput(net + " reference output", board.aux, a);
                        graph.Load(net + " fixture input", b, board.ground, 10000f);
                        if (i == 30) { board.clockDriveP = drive; board.clockP = b; }
                        else { board.clockDriveN = drive; board.clockN = b; }
                    }
                    else if (choke)
                    {
                        graph.SwitchedOutput("L1 filter measurement stimulus", board.aux, a, true);
                        board.chokeSense = graph.Pin("U18.L1_BIAS_WITNESS");
                        var feed = graph.Load("Filter witness feed", b, board.chokeSense, 100f);
                        board.chokeSense.transform.SetParent(feed.transform, true);
                        graph.Load("Filter witness reference", board.chokeSense, board.ground, 10000f);
                        var shunt = graph.Pin("L1.SATURATION_SHUNT");
                        board.chokeShunt = graph.SwitchedOutput("L1 saturation under stress current", board.chokeSense, shunt);
                        graph.Load("Saturated bias load", shunt, board.ground, 49.9f);
                    }
                    else if (shield)
                    {
                        board.shieldSense = a;
                        board.shieldExcitation = graph.Load("U20 Kelvin stimulus resistor", board.aux, a, 1500f);
                        board.shieldBond = graph.Contact("Shield can 1 seam bond", b, board.ground);
                        Set(board.shieldBond, ("closedResistance", .016f));
                        graph.Load("Secondary shield strap", b, board.ground, .45f);
                    }
                    else if (stitch)
                    {
                        board.returnRoute = i;
                        route.viaId = StitchVia;
                        route.exitLayer = 7;
                        route.barrel = graph.SwitchedOutput("V44 L06 to L07 ground stitch", b, board.returnSense);
                        graph.Load("Return plane tie", a, board.ground, 1f);
                        graph.Load("U12 return probe bias", board.aux, board.returnSense, 2200f);
                    }
                    else if (dataLeg >= 0)
                    {
                        var lane = board.lanes[dataLeg / 2];
                        int polarity = dataLeg % 2;
                        lane.drive[polarity] = graph.SwitchedOutput(net + " diagnostic symbol driver", board.aux, a);
                        lane.sense[polarity] = b;
                        graph.Load(net + " receiver load", b, board.ground, 10000f);
                    }
                    else if (quota > 0)
                    {
                        quota--;
                        route.viaId = remaining.Dequeue();
                        route.layer = i % 15 + 1;
                        route.exitLayer = route.layer + 1;
                        previous = graph.Pin(route.viaId + $".L{route.exitLayer:00}", end);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", b, previous);
                    }
                    routes.Add(route);
                    csv.Add(Row(route));
                }
                graph.Load(group.Key + " continuity witness", previous, board.ground, 1000f);
                witnesses.Add(previous);
            }
            if (remaining.Count != 0 || routes.Count != SparkLevel112Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel112Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != SparkLevel112Process.LayerCount ||
                board.parts.Any(part => !part.contact) || board.lanes.Any(lane => lane.sense.Any(pin => !pin)))
                throw new InvalidOperationException("Incomplete EMC-20 training topology.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();

            Vector3 from = Site("U21") + new Vector3(-.008f, .003f, .010f);
            Vector3 to = Site("U9") + new Vector3(.008f, .003f, .010f);
            var sender = graph.Pin("U21.EMC_QUALIFIED_OUT", from, true);
            var receiver = graph.Pin("U9.EMC_QUALIFIED_IN", to, true);
            board.heartbeat = graph.SwitchedOutput("U21 trusted spectrum cadence", board.aux, sender);
            var bias = graph.Load("U9 feedback pull-down", receiver, board.ground, 10000f);
            receiver.transform.SetParent(bias.transform, true);
            board.jumper = new SparkLevel112Process.Route
            {
                id = "W209", group = "Independent safety", net = "EMC_QUALIFIED", layer = 1,
                from = sender, to = receiver, entry = from, destination = to, copper = Copper(graph.Root, mats["Gold_ENIG"], "W209", from, to)
            };
            csv.Add(Row(board.jumper));
            board.monitorSupply = graph.Pin("U9.INDEPENDENT_3V3");
            var monitor = graph.Load("U9 independent supply", board.aux, board.monitorSupply, 1f);
            board.monitorSupply.transform.SetParent(monitor.transform, true);
            graph.Load("U9 supply witness", board.monitorSupply, board.ground, 1000f);
            board.protectedFeed = graph.Pin("U9.PROTECTED_EMISSION_WORKLOAD");
            board.workload = graph.SwitchedOutput("U9 emissions workload relay", board.monitorSupply, board.protectedFeed);
            graph.Load("Protected workload witness", board.protectedFeed, board.ground, 330f);
            foreach (Vector3 point in new[] { from, to })
                Primitive(graph.Root, "W209 contact", PrimitiveType.Cylinder, point, new Vector3(.003f, .00024f, .003f), mats["Gold_ENIG"]);

            board.startMarker = Marker(graph.Root, "Route source", mats["Gold_ENIG"], .003f);
            board.endMarker = Marker(graph.Root, "Route destination", mats["Signal_Cyan"], .003f);
            board.locationMarker = Marker(graph.Root, "Placement locator", mats["Warning_Amber"], .0048f);
            board.statusLamp = Primitive(graph.Root, "U9 permit lamp", PrimitiveType.Cube, to + new Vector3(.005f, .001f, 0),
                new Vector3(.0025f, .0007f, .0018f), mats["Signal_Cyan"]);
            board.faultOverlays = new[] { shieldSite, Site("L1"), routes[board.returnRoute].destination }.Select((point, i) =>
            {
                var marker = Marker(graph.Root, "Measured fault " + (i + 1), mats["Fault_Red"], .0059f);
                marker.transform.localPosition = point + Vector3.up * .011f;
                marker.enabled = true;
                marker.gameObject.SetActive(false);
                return marker.gameObject;
            }).ToArray();
            var silk = new Color(.74f, .85f, .91f);
            BoardText(graph.Root, "EMC-20  /  THE EMC GAUNTLET", new Vector3(-.105f, .004f, -.109f), .00105f, Color.white);
            BoardText(graph.Root, "W209  /  U21 → U9", (from + to) * .5f + new Vector3(.013f, .0017f, 0), .00063f, silk);
            BoardText(graph.Root, "L1  CM FILTER", Site("L1") + new Vector3(0, .0034f, -.012f), .00062f, silk);
            BoardText(graph.Root, "CAN 1  BOND", shieldSite + new Vector3(0, .0034f, -.015f), .00063f, silk);
            WorkLight(graph.Root, "Chamber key", new Vector3(-.094f, .112f, -.046f), new Color(1f, .94f, .85f), 10.4f);
            WorkLight(graph.Root, "Probe fill", new Vector3(.094f, .109f, .057f), new Color(.69f, .86f, 1f), 10.5f);
            return csv;
        }

        private static string Row(SparkLevel112Process.Route route) => FormattableString.Invariant(
            $"{route.id},{route.group},{route.net},{route.layer},{route.from.name},{route.to.name},{route.entry.x * 1000:F3},{route.entry.z * 1000:F3},{route.destination.x * 1000:F3},{route.destination.z * 1000:F3},{route.viaId},{route.exitLayer}");

        private static Vector3 FindPad(Vector3 site, int angleOffset, List<Vector3> used)
        {
            for (int ring = 0; ring < 10; ring++)
            for (int step = 0; step < 8; step++)
            {
                float angle = (angleOffset + step) * Mathf.PI / 4f;
                float radius = .0048f + .0023f * ring;
                var point = new Vector3(site.x + radius * Mathf.Cos(angle), .0046f, site.z + radius * Mathf.Sin(angle));
                if (Mathf.Abs(point.x) > .175f || Mathf.Abs(point.z) > .120f ||
                    used.Exists(other => (other - point).sqrMagnitude < .0000027f)) continue;
                used.Add(point);
                return point;
            }
            throw new InvalidOperationException("No free EMC-20 training terminal near " + site);
        }
    }
}
