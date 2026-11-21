using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.AxisForge
{
    public sealed partial class SparkLevel120Process
    {
        public const string ManifestSha256 = "5a8938f1c8b6a36dbfa9f80fc682fa75fb2ee18938dd747cfaa39ee5d61a507c";
        public const string CheckpointKey = "ProjectSpark.Level120.RAX28.v1";
        private float saveAt = float.PositiveInfinity;
        private bool restoring;

        [Serializable]
        private sealed class Checkpoint
        {
            public string design, manifest;
            public bool[] parts, routes, vias;
            public bool jumper, interrupted, observed, reworked;
            public int history, repaired, fault;
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
                design = Design, manifest = ManifestSha256, history = stage, repaired = repaired, fault = activeFault,
                parts = Array.ConvertAll(parts, part => part.contact.IsClosed),
                routes = Array.ConvertAll(routes, HasWire),
                vias = Array.ConvertAll(routes, route => route.barrel && route.barrel.IsClosed),
                jumper = HasWire(jumper), interrupted = interrupted, observed = faultWitnessed,
                reworked = reworked
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
            if (saved == null || saved.design != Design || saved.manifest != ManifestSha256 || saved.parts?.Length != PartCount ||
                saved.routes?.Length != RouteCount ||
                saved.vias?.Length != RouteCount || saved.history is < 0 or > 6 || saved.repaired is < 0 or > 3 ||
                saved.fault != -1 && (saved.fault != saved.repaired || saved.fault >= 3))
                return Reject("The checkpoint belongs to a different training revision.");

            restoring = true;
            try
            {
                using (manager.ElectricalSolver.BeginConfigurationBatch())
                {
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (!saved.parts[i]) continue;
                        SetContact(parts[i].contact, true);
                        parts[i].model.SetActive(i != jumperPart);
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
                }
                ReadGraph();
                bool continuity = installed == PartCount && routed == RouteCount && plated == ViaCount &&
                    Array.TrueForAll(witnesses, pin => Voltage(pin) > 11.4f);
                int history = installed == PartCount ? 1 : 0;
                if (continuity && (linked || saved.history >= 5 && saved.interrupted))
                    history = Mathf.Clamp(saved.history, 1, 5);
                if (history >= 4)
                {
                    repaired = saved.repaired;
                    activeFault = saved.fault;
                    faultWitnessed = saved.observed;
                    reworked = saved.reworked;
                    repairTarget = faultWitnessed ? ExpectedPart : -1;
                }
                interrupted = history == 5 && saved.interrupted;
                minimumEpoch = interrupted ? epoch + 1 : 0;
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
                message = "Checkpoint restored with power isolated. Precharge, prove both STO channels, home all joints and run a fresh coordinated path.";
                return true;
            }
            finally { restoring = false; }
        }
    }
}
