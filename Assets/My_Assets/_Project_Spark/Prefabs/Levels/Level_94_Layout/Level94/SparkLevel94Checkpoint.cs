using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel94Process
    {
        public const string CheckpointKey = "ProjectSpark.Level94.BoardFoundry.v1";
        private float checkpointDue = float.PositiveInfinity;
        private bool restoring;
        private string pendingCheckpoint;

        [Serializable]
        private sealed class Checkpoint
        {
            public int stage, fault, repairedPending;
            public bool interrupted;
            public bool[] inspected, parts, front, back, via, repairs;
        }

        private bool CanResume => stage == 0 && inspected.Count == 0 && PlayerPrefs.HasKey(CheckpointKey);

        private void QueueCheckpoint()
        {
            if (restoring || !Active) return;
            ReadGraph();
            var reviewed = new bool[CopperCount];
            foreach (int index in inspected) reviewed[index] = true;
            var record = new Checkpoint
            {
                stage = stage,
                fault = fault,
                repairedPending = repairedPending,
                interrupted = lost || reset || stage == Gates.Length,
                inspected = reviewed,
                parts = Array.ConvertAll(parts, item => item.activeSelf),
                front = Array.ConvertAll(routes, item => Edge(item.from, item.via ? item.viaTop : item.to)),
                back = Array.ConvertAll(routes, item => item.via && Edge(item.viaBottom, item.to)),
                via = Array.ConvertAll(routes, item => item.via && item.via.IsClosed),
                repairs = repairs
            };
            // Capture while the layout still owns its native connections. The loader
            // removes them before deactivating the layout both on restart and on exit.
            pendingCheckpoint = JsonUtility.ToJson(record);
            checkpointDue = Time.unscaledTime + .35f;
        }

        private void FlushCheckpoint()
        {
            if (pendingCheckpoint == null) return;
            PlayerPrefs.SetString(CheckpointKey, pendingCheckpoint);
            PlayerPrefs.Save();
            pendingCheckpoint = null;
            checkpointDue = float.PositiveInfinity;
        }

        public void ResumeCheckpoint()
        {
            if (!Active || !CanResume) return;
            Checkpoint saved;
            try { saved = JsonUtility.FromJson<Checkpoint>(PlayerPrefs.GetString(CheckpointKey)); }
            catch (ArgumentException)
            {
                notice = "The checkpoint is unreadable.";
                return;
            }
            if (saved?.inspected?.Length != CopperCount || saved.parts?.Length != parts.Length ||
                saved.front?.Length != routes.Length || saved.back?.Length != routes.Length ||
                saved.via?.Length != routes.Length || saved.repairs?.Length != repairs.Length ||
                saved.stage is < 0 or > 6 || saved.fault is < -1 or > 2 ||
                saved.repairedPending is < -1 or > 2)
            {
                notice = "The checkpoint is incompatible with this Board Foundry revision.";
                return;
            }

            restoring = true;
            var created = new List<ulong>();
            try
            {
                int history = 0;
                for (int i = 0; i < saved.inspected.Length; i++)
                    if (saved.inspected[i]) inspected.Add(i);
                if (saved.stage > 0 && inspected.Count == CopperCount)
                {
                    history = 1;
                    for (int i = 0; i < parts.Length; i++)
                        if (saved.parts[i]) SetInstalled(i, true);
                }
                if (history == 1 && saved.stage > 1 && InstalledCount == parts.Length)
                {
                    history = 2;
                    for (int i = 0; i < routes.Length; i++)
                    {
                        var item = routes[i];
                        if (item.via && saved.via[i]) item.via.SetState(SparkSwitchState.Closed);
                        if (saved.front[i] && !RestoreWire(item.from, item.via ? item.viaTop : item.to, created) ||
                            item.via && saved.back[i] && !RestoreWire(item.viaBottom, item.to, created))
                        {
                            RollBackResume(created);
                            notice = "A checkpoint connection was rejected. The board is still isolated.";
                            return;
                        }
                    }
                    ReadGraph();
                    bool openGroundFault = saved.stage == 4 && saved.fault == 0 && routed == CopperCount - 1 &&
                        routes[29].via && !routes[29].via.IsClosed;
                    if (saved.stage >= 3 && (routed == CopperCount || openGroundFault)) history = 3;
                    if (history == 3 && saved.stage >= 4 &&
                        (RouteConnected(routes[JumperIndex]) || saved.stage >= 5 && saved.interrupted)) history = 4;
                    if (history == 4 && saved.stage >= 5 && Array.TrueForAll(saved.repairs, value => value)) history = 5;
                }
                if (history >= 4)
                {
                    Array.Copy(saved.repairs, repairs, repairs.Length);
                    fault = saved.fault;
                    repairedPending = saved.repairedPending;
                    if (fault == 1) pullup.SetResistance(47000f);
                    if (fault == 2) bridge.SetState(SparkSwitchState.Closed);
                }
                lost = history >= 5 && saved.interrupted;
                while (stage < history && Award(false)) { }
                route = stage >= 3 ? JumperIndex : 0;
                if (stage is 0 or 2)
                    for (int i = 0; i < CopperCount; i++)
                    {
                        if (stage == 0 ? inspected.Contains(i) : RouteConnected(routes[i])) continue;
                        route = i;
                        break;
                    }
                if (stage == 1)
                {
                    part = Array.FindIndex(parts, item => !item.activeSelf);
                    part = Mathf.Max(part, 0);
                    partPick = part;
                    ResetPartSelection();
                }
                notice = "Checkpoint restored with power isolated. Collect fresh measurements before qualification.";
                BoardView();
            }
            finally { restoring = false; }
        }

        private bool RestoreWire(SparkTerminal a, SparkTerminal b, List<ulong> created)
        {
            if (!Connect(a, b, out ulong id)) return false;
            if (id != 0) created.Add(id);
            return true;
        }

        private void RollBackResume(List<ulong> created)
        {
            foreach (ulong id in created) Remove(id);
            foreach (var item in routes)
                if (item.via) item.via.SetState(SparkSwitchState.Open);
            for (int i = 0; i < parts.Length; i++) SetInstalled(i, false);
            inspected.Clear();
            ReadGraph();
        }
    }
}
