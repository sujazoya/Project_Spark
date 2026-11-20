using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.SignalPhantom
{
    public sealed partial class SparkLevel102Process
    {
        public const string CheckpointKey = "ProjectSpark.Level102.HSX10.v1";
        private float saveAt = float.PositiveInfinity;
        private bool restoring;

        [Serializable]
        private sealed class Checkpoint
        {
            public string design;
            public bool[] parts, wires, vias;
            public bool jumper, observed, couplingWitness, repairedContact, interrupted;
            public int history, repaired, fault;
        }

        private void QueueCheckpoint()
        {
            if (!restoring) saveAt = time + .5f;
        }

        private void SavePendingCheckpoint()
        {
            if (float.IsPositiveInfinity(saveAt) || !manager || !manager.CircuitSystem) return;
            var checkpoint = new Checkpoint
            {
                design = Design, history = stage, repaired = repaired, fault = activeFault,
                observed = faultWitnessed, couplingWitness = couplingIsolated, repairedContact = reworked, interrupted = interrupted,
                parts = Array.ConvertAll(parts, part => part.contact.IsClosed),
                wires = Array.ConvertAll(routes, HasWire),
                vias = Array.ConvertAll(routes, route => route.barrel && route.barrel.IsClosed),
                jumper = HasWire(jumper)
            };
            PlayerPrefs.SetString(CheckpointKey, JsonUtility.ToJson(checkpoint));
            PlayerPrefs.Save();
            saveAt = float.PositiveInfinity;
        }

        public bool Resume()
        {
            if (stage != 0 || installed != 0 || !PlayerPrefs.HasKey(CheckpointKey))
                return Reject("Resume is available on an empty board with a saved checkpoint.");
            Checkpoint saved;
            try { saved = JsonUtility.FromJson<Checkpoint>(PlayerPrefs.GetString(CheckpointKey)); }
            catch (ArgumentException) { return Reject("The saved checkpoint is unreadable."); }
            if (saved == null || saved.design != Design || saved.parts?.Length != PartCount ||
                saved.wires?.Length != RouteCount || saved.vias?.Length != RouteCount ||
                saved.history is < 0 or > 6 || saved.repaired is < 0 or > 3 ||
                saved.fault != -1 && (saved.fault != saved.repaired || saved.fault >= 3) ||
                saved.history < 5 && saved.interrupted || saved.history >= 5 && saved.repaired != 3 ||
                saved.repairedContact && !saved.observed)
                return Reject("The checkpoint is incompatible with this HSX-10 revision.");

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
                        if (saved.wires[i]) CreateWire(route);
                    }
                    if (saved.jumper) CreateWire(jumper);
                }
                ReadGraph();
                Solve();
                bool continuity = Construction && Array.TrueForAll(witnesses, pin => Voltage(pin) > 4.7f);
                int history = installed == PartCount ? 1 : 0;
                if (continuity && (linked || saved.history >= 5 && saved.interrupted))
                    history = Mathf.Clamp(saved.history, 1, 5);
                if (history >= 4)
                {
                    repaired = saved.repaired;
                    activeFault = saved.fault;
                    faultWitnessed = saved.observed;
                    couplingIsolated = saved.fault == 1 && saved.observed && saved.couplingWitness;
                    reworked = saved.repairedContact;
                    ConfigureFaultDevices();
                }
                interrupted = history == 5 && saved.interrupted;
                while (stage < history && Award()) {}
                InvalidateBoot();
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
                message = "Checkpoint restored with power isolated. Recalibrate and train both lanes before running fresh tests.";
                return true;
            }
            finally { restoring = false; }
        }
    }
}
