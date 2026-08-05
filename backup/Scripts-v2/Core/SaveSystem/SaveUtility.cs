using System.IO;

namespace ProjectSpark.Core.SaveSystem
{
    public static class SaveUtility
    {
        public static bool Exists(
            int slot)
        {
            return File.Exists(
                SavePaths.Slot(slot));
        }

        public static void Delete(
            int slot)
        {
            string path =
                SavePaths.Slot(slot);

            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
