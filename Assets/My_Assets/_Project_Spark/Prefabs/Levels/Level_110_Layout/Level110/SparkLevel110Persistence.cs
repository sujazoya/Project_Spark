using System;
using System.Linq;
using ProjectSpark.Circuit;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel110Process
    {
        internal const string BoardSaveKey = "ProjectSpark.Level110.Board.v1";

        [Serializable] private sealed class BoardSave
        {
            public string design;
            public int checkpoint;
            public bool coolingMounted;
            public bool[] placed, soldered, links, vias, repairs;
        }

        public void SaveBoard()
        {
            if (!ready) return;
            var save = new BoardSave
            {
                design = Design,
                checkpoint = Math.Min(stage, 5),
                coolingMounted = coolingMounted,
                placed = Enumerable.Range(0, parts.Length).Select(placed.Contains).ToArray(),
                soldered = Enumerable.Range(0, parts.Length).Select(soldered.Contains).ToArray(),
                links = routes.Append(jumper).Select(HasEdge).ToArray(),
                vias = routes.Where(route => route.via).Select(route => route.via.IsConducting).ToArray(),
                repairs = (bool[])repaired.Clone()
            };
            PlayerPrefs.SetString(BoardSaveKey, JsonUtility.ToJson(save));
            PlayerPrefs.Save();
        }

        public bool ResumeBoard()
        {
            if (!ready || powered || Installed != 0 || placed.Count != 0 || stage != 0) return false;
            BoardSave save;
            try { save = JsonUtility.FromJson<BoardSave>(PlayerPrefs.GetString(BoardSaveKey)); }
            catch (ArgumentException) { return false; }
            if (save == null || save.design != Design || save.checkpoint < 0 || save.checkpoint > 5
                || save.placed?.Length != parts.Length || save.soldered?.Length != parts.Length
                || save.links?.Length != routes.Length + 1 || save.vias?.Length != 56 || save.repairs?.Length != 3
                || save.soldered.Where((value, index) => value && !save.placed[index]).Any()
                || save.coolingMounted && !save.soldered.All(value => value)
                || save.checkpoint >= 1 && !save.coolingMounted
                || save.checkpoint >= 2 && (!save.links.Take(routes.Length).All(value => value) || !save.vias.All(value => value))
                || save.checkpoint >= 4 && !save.repairs.All(value => value)) return false;

            var allRoutes = routes.Append(jumper).ToArray();
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                // Restore edges before parts. A rejected edge leaves the board in its initial state.
                var added = new System.Collections.Generic.List<ulong>();
                for (int i = 0; i < allRoutes.Length; i++)
                {
                    var route = allRoutes[i];
                    if (!save.links[i]) continue;
                    if (!manager.CircuitSystem.TryCreateConnection(route.from, route.to, SparkConnectionKind.Wire,
                        SparkConnectionDirection.Bidirectional, out var connection))
                    {
                        foreach (ulong id in added) manager.CircuitSystem.TryRemoveConnection(id, out _);
                        return false;
                    }
                    added.Add(connection.Id);
                }
                for (int i = 0; i < parts.Length; i++)
                {
                    if (save.placed[i]) { placed.Add(i); parts[i].visual.SetActive(true); }
                    if (save.soldered[i]) { soldered.Add(i); parts[i].contact.SetState(SparkSwitchState.Closed); }
                }
                int viaIndex = 0;
                foreach (var route in routes)
                    if (route.via && save.vias[viaIndex++]) route.via.SetState(SparkSwitchState.Closed);
            }

            Array.Copy(save.repairs, repaired, repaired.Length);
            ReadTopology();
            // Level 3 and later must be re-earned from fresh model evidence.
            if (save.coolingMounted) MountCooling();
            if (Installed == parts.Length && coolingMounted) Award();
            if (stage == 1 && routed == routes.Length && plated == 56 && jumperPresent && healthy && !unauthorized) Award();
            int next = Array.FindIndex(parts, item => !item.contact.IsClosed);
            if (next >= 0) { SelectInventory(next); SelectFootprint(next); }
            message = "Board restored. Electrical qualification and safety must be measured again.";
            return true;
        }

        private void ResumeOrSave()
        {
            if (PlacedOrProgressed())
            {
                SaveBoard();
                message = "Board saved.";
            }
            else if (!ResumeBoard()) message = "No compatible saved board could be loaded.";
        }

        private bool PlacedOrProgressed() => placed.Count != 0 || stage != 0;
    }
}
