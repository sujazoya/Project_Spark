using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.SecureEnclave
{
    public sealed partial class SparkLevel115Process
    {
        public const string CheckpointKey = "ProjectSpark.Level115.SHX23.v1";
        private float saveAt = float.PositiveInfinity;
        private bool restoring;

        [Serializable]
        private sealed class Checkpoint
        {
            public string design;
            public bool[] parts, mounted, routes, vias;
            public bool jumper, interrupted, observed, reworked, replacementPrepared;
            public int history, repaired, fault, boot, version;
            public bool tamper;
        }

        private void QueueCheckpoint()
        {
            if (!restoring) saveAt = time + .5f;
        }

        private void SavePendingCheckpoint()
        {
            if (float.IsPositiveInfinity(saveAt) || !manager || !manager.CircuitSystem) return;
            var saved = new Checkpoint
            {
                design = Design, history = stage, repaired = repaired, fault = activeFault,
                parts = Array.ConvertAll(parts, part => part.contact.IsClosed),
                mounted = Array.ConvertAll(shieldParts, Mounted),
                routes = Array.ConvertAll(routes, HasWire),
                vias = Array.ConvertAll(routes, route => route.barrel && route.barrel.IsClosed),
                jumper = HasWire(jumper), interrupted = interrupted, observed = faultWitnessed,
                reworked = reworked, replacementPrepared = replacementPrepared, boot = bootEpoch, version = versionFloor,
                tamper = tamperLatched
            };
            PlayerPrefs.SetString(CheckpointKey, JsonUtility.ToJson(saved));
            PlayerPrefs.Save();
            saveAt = float.PositiveInfinity;
        }

        public bool Resume()
        {
            if (stage != 0 || installed != 0 || !PlayerPrefs.HasKey(CheckpointKey))
                return Reject("Resume is available on an empty board with a saved checkpoint.");
            Checkpoint saved;
            try { saved = JsonUtility.FromJson<Checkpoint>(PlayerPrefs.GetString(CheckpointKey)); }
            catch (ArgumentException) { return Reject("The checkpoint could not be read."); }
            if (saved == null || saved.design != Design || saved.parts?.Length != PartCount ||
                saved.mounted?.Length != shieldParts.Length || saved.routes?.Length != RouteCount ||
                saved.vias?.Length != RouteCount || saved.history is < 0 or > 6 || saved.repaired is < 0 or > 3 ||
                saved.fault != -1 && (saved.fault != saved.repaired || saved.fault >= 3) ||
                saved.boot is < 0 or > 1000000 || saved.version is < 0 or > 3)
                return Reject("The checkpoint belongs to a different training revision.");

            restoring = true;
            try
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (!saved.parts[i]) continue;
                    SetContact(parts[i].contact, true);
                    int cooling = Array.IndexOf(shieldParts, i);
                    if (cooling >= 0) SetMounted(i, saved.mounted[cooling]);
                    else parts[i].model.SetActive(i != jumperPart);
                    installed++;
                }
                if (installed == PartCount)
                {
                    for (int i = 0; i < routes.Length; i++)
                    {
                        var route = routes[i];
                        if (route.barrel && saved.vias[i]) SetContact(route.barrel, true);
                        if (saved.routes[i] && (!route.barrel || route.barrel.IsClosed)) CreateWire(route);
                    }
                    if (saved.jumper) CreateWire(jumper);
                }
                ReadGraph();
                Solve();
                bool continuity = installed == PartCount && routed == RouteCount && plated == ViaCount &&
                    Array.TrueForAll(witnesses, pin => Voltage(pin) > 4.7f);
                int history = installed == PartCount ? 1 : 0;
                if (continuity && (linked || saved.history >= 5 && saved.interrupted))
                    history = Mathf.Clamp(saved.history, 1, 5);
                bootEpoch = saved.boot;
                versionFloor = saved.version;
                if (history >= 4)
                {
                    repaired = saved.repaired;
                    activeFault = saved.fault;
                    faultWitnessed = saved.observed;
                    reworked = saved.reworked;
                    replacementPrepared = saved.replacementPrepared;
                    tamperLatched = saved.tamper;
                    if (FaultLive(2)) SetContact(shieldSeams[2], false);
                    repairTarget = faultWitnessed ? ExpectedPart : -1;
                }
                interrupted = history == 5 && saved.interrupted;
                minimumBoot = interrupted ? bootEpoch + 1 : 0;
                while (stage < history && Award()) { }
                InvalidateDiagnostics();
                BoardView();
                if (stage == 0)
                {
                    int next = Array.FindIndex(parts, part => !part.contact.IsClosed);
                    SelectFootprint(next);
                    SelectPart(next);
                }
                else if (stage == 1)
                {
                    SelectRoute(routes.Length);
                    SelectNextRoute();
                }
                message = "Checkpoint restored with power isolated. Cold boot and acquire a fresh signed challenge to re-establish trust.";
                return true;
            }
            finally { restoring = false; }
        }
    }
}
