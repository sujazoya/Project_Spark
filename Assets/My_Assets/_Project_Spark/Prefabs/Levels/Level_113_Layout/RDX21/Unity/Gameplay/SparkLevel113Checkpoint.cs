using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.RedundancyCitadel
{
    public sealed partial class SparkLevel113Process
    {
        public const string CheckpointKey = "ProjectSpark.Level113.RDX21.v1";
        private float saveAt = float.PositiveInfinity;
        private bool restoring;

        [Serializable]
        private sealed class Checkpoint
        {
            public string design;
            public bool[] parts, routes, vias;
            public bool jumper, interrupted, inhibitionObserved;
            public int history, repaired;
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
                design = Design, history = stage, repaired = repaired,
                parts = Array.ConvertAll(parts, part => part.contact.IsClosed),
                routes = Array.ConvertAll(routes, HasWire),
                // Resume unsigned fault trials from the last repaired state.
                vias = Array.ConvertAll(routes, route => route.barrel &&
                    (route.barrel.IsClosed || route == EccVia && ExpectedEccGap)),
                jumper = HasWire(jumper), interrupted = interrupted, inhibitionObserved = inhibitionObserved
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
                saved.routes?.Length != RouteCount ||
                saved.vias?.Length != RouteCount || saved.history is < 0 or > 6 || saved.repaired is < 0 or > 3)
                return Reject("The checkpoint belongs to a different training revision.");

            restoring = true;
            try
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
                ReadGraph();
                Solve();
                bool continuity = installed == PartCount && routed == RouteCount && plated == ViaCount &&
                    ContinuityVerified();
                int history = installed == PartCount ? 1 : 0;
                if (continuity && (linked || saved.history >= 5 && saved.interrupted))
                    history = Mathf.Clamp(saved.history, 1, 5);
                if (history >= 4) repaired = saved.repaired;
                interrupted = history == 5 && saved.interrupted;
                latch = interrupted;
                inhibitionObserved = interrupted && saved.inhibitionObserved;
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
                message = "Checkpoint restored with power isolated. Capture fresh qualification evidence for this session.";
                return true;
            }
            finally { restoring = false; }
        }
    }
}

