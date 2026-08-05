using System;

namespace ProjectSpark.Core.SaveSystem
{
    public static class SaveEvents
    {
        public static event Action Saved;

        public static event Action Loaded;

        public static event Action AutoSaved;

        public static void RaiseSaved()
        {
            Saved?.Invoke();
        }

        public static void RaiseLoaded()
        {
            Loaded?.Invoke();
        }

        public static void RaiseAutoSaved()
        {
            AutoSaved?.Invoke();
        }
    }
}
