using System;
using ProjectSpark.Circuit;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel107Process
    {
        public const string CheckpointKey = "ProjectSpark.Level107.PFX15.v1";
        private float saveAt = float.PositiveInfinity;
        private bool restoring;

        [Serializable]
        private sealed class Checkpoint
        {
            public string design, primary;
            public bool[] contacts, wires, vias;
            public bool linked, initialRestored, cutObserved, csFault, pullupFault, witnessed, reworked, interrupted;
            public int stage, repaired, incident;
        }

        private void QueueCheckpoint()
        {
            if (!restoring) saveAt = time + .70f;
        }

        private void FlushCheckpoint()
        {
            if (saveAt == float.PositiveInfinity || !manager || !Application.isPlaying) return;
            var saved = new Checkpoint
            {
                design = Design, primary = flash.ExportPrimary(), stage = stage, repaired = repaired, incident = fault,
                linked = HasWire(jumper), initialRestored = initialRestored, cutObserved = cutObserved,
                csFault = chipSelectFault, pullupFault = pullupFault, witnessed = causeObserved,
                reworked = reworked, interrupted = safetyInterrupted,
                contacts = Array.ConvertAll(parts, part => part.solder.IsClosed),
                wires = Array.ConvertAll(routes, HasWire),
                vias = Array.ConvertAll(routes, route => route.via && route.via.IsClosed)
            };
            PlayerPrefs.SetString(CheckpointKey, JsonUtility.ToJson(saved));
            PlayerPrefs.Save();
            saveAt = float.PositiveInfinity;
        }

        public void RestoreCheckpoint()
        {
            if (stage != 0 || installed != 0 || !PlayerPrefs.HasKey(CheckpointKey))
            { message = "Resume is available from an untouched board with a saved checkpoint."; return; }
            Checkpoint saved;
            try { saved = JsonUtility.FromJson<Checkpoint>(PlayerPrefs.GetString(CheckpointKey)); }
            catch (ArgumentException) { message = "The checkpoint could not be decoded."; return; }
            if (saved == null || saved.design != Design || saved.contacts?.Length != parts.Length ||
                saved.wires?.Length != routes.Length || saved.vias?.Length != routes.Length ||
                saved.stage is < 0 or > 6 || saved.incident is < -1 or > 2 || saved.repaired is < 0 or > 3 ||
                !flash.RestorePrimary(saved.primary))
            { message = "The checkpoint is incompatible with this PFX-15 revision."; return; }

            restoring = true;
            try
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (!saved.contacts[i]) continue;
                    SetContact(parts[i].solder, true);
                    parts[i].model.SetActive(i != jumperIndex);
                    installed++;
                }
                if (Installed == parts.Length)
                {
                    for (int i = 0; i < routes.Length; i++)
                    {
                        var route = routes[i];
                        if (route.via) SetContact(route.via, saved.vias[i]);
                        if (saved.wires[i]) RestoreWire(route);
                    }
                    if (saved.linked) RestoreWire(jumper);
                }
                ReadGraph();
                manager.ElectricalSolver.SolveNow();
                int history = installed != parts.Length ? 0 : !Construction ? 1 : Mathf.Min(saved.stage, 5);
                while (stage < history && Award()) {}
                if (history >= 2)
                {
                    repaired = saved.repaired;
                    fault = saved.incident;
                    initialRestored = saved.initialRestored;
                    cutObserved = saved.cutObserved;
                    chipSelectFault = saved.csFault;
                    pullupFault = saved.pullupFault;
                    causeObserved = saved.witnessed;
                    reworked = saved.reworked;
                    safetyInterrupted = saved.interrupted;
                }
                primaryText = "Unchecked";
                recoveryText = "Unchecked";
                InvalidateTrust();
                BoardView();
                if (stage == 0)
                {
                    int index = Array.FindIndex(parts, part => !part.solder.IsClosed);
                    SelectFootprint(index);
                    SelectPart(index);
                }
                else if (stage == 1) SelectNextRoute();
                message = "Checkpoint restored with bench isolation. Live bank, POST and safety evidence must be collected again.";
            }
            finally { restoring = false; }
        }

        private void RestoreWire(Route route) => manager.CircuitSystem.TryCreateConnection(route.from, route.to,
            SparkConnectionKind.Wire, SparkConnectionDirection.Bidirectional, out _);
    }
}
