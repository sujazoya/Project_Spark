using System.IO;
using UnityEngine;

namespace ProjectSpark.Core.SaveSystem
{
    public sealed class SaveSerializer
    {
        public void Save(
            SaveGame game,
            int slot)
        {
            Directory.CreateDirectory(
                SavePaths.SaveDirectory);

            string json =
                JsonUtility.ToJson(
                    game,
                    true);

            File.WriteAllText(
                SavePaths.Slot(slot),
                json);
        }

        public SaveGame Load(
            int slot)
        {
            string path =
                SavePaths.Slot(slot);

            if (!File.Exists(path))
                return null;

            return JsonUtility.FromJson<SaveGame>(
                File.ReadAllText(path));
        }
    }
}
