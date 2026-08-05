using System.IO;
using UnityEngine;

namespace ProjectSpark.Core.SaveSystem
{
    public static class SavePaths
    {
        public static string SaveDirectory =>
            Path.Combine(
                Application.persistentDataPath,
                "Saves");

        public static string Slot(int slot)
        {
            return Path.Combine(
                SaveDirectory,
                $"slot_{slot}.json");
        }
    }
}
